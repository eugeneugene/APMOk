using ConfigTest.JsonExtensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ConfigTest
{
    public class WritableOptions<T1> : IWritableOptions<T1>
        where T1 : class, new()
    {
        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };

        private readonly IHostEnvironment _environment;
        private readonly IOptionsMonitor<T1> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableOptions(
            IHostEnvironment environment,
            IOptionsMonitor<T1> options,
            string section,
            string file)
        {
            _environment = environment;
            _options = options;
            _section = section;
            _file = file;
        }

        public T1 Value => _options.CurrentValue;
        public T1 Get(string name) => _options.Get(name);
        private readonly T1 empty = new();

        private string FilePath
        {
            get { }
        }

        public void Update(Action<T1> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            if (!File.Exists(physicalPath))
            {
                Create(applyChanges);
                return;
            }

            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(physicalPath);
            if (jsonReadOnlySpan.StartsWith(Utf8Bom))
                jsonReadOnlySpan = jsonReadOnlySpan[Utf8Bom.Length..];
            byte[] Utf8Section = Encoding.UTF8.GetBytes(_section);

            var reader = new Utf8JsonReader(jsonReadOnlySpan);

            int count = 0;
            int total = 0;

            while (reader.Read())
            {
                JsonTokenType tokenType = reader.TokenType;

                switch (tokenType)
                {
                    case JsonTokenType.StartObject:
                        total++;
                        break;
                    case JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals(Utf8Section))
                        {
                            // Assume valid JSON, known schema
                            reader.Read();
                            if (reader.GetString().EndsWith("University"))
                            {
                                count++;
                            }
                        }
                        break;
                }
                Console.WriteLine($"{count} out of {total}");
            }
        }

        public void Create(Action<T1> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            var serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true, 
            };

            var stream = File.Create(physicalPath);

            T1 obj = new();
            applyChanges(obj);
            
            var barray = JsonSerializer.SerializeToUtf8Bytes(obj, serializerOptions);
            stream.Write(barray, 0, barray.Length);
            stream.Close();
        }

        public void Update1(Action<T1> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            using var document = File.Exists(physicalPath) ? JsonDocument.Parse(File.ReadAllText(physicalPath)) : empty.ToJsonDocument(null);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Document is not an object");

            List<JsonProperty> collection = new();
            foreach (var item in root.EnumerateObject())
            {
                if (item.Name == _section)
                {
                    var s = item.Value.ToObject<T1>();
                    applyChanges(s);
                    var document1 = s.ToJsonDocument(null);
                    var root1 = document1.RootElement;
                    if (root1.ValueKind != JsonValueKind.Object)
                        throw new InvalidOperationException("Document is not an object");
                    foreach (var item1 in root1.EnumerateObject())
                        collection.Add(item1);
                }
                else
                    collection.Add(item);
            }

            using FileStream stream = new(physicalPath, FileMode.Create);
            using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();
            foreach (var item in collection)
                item.WriteTo(writer);
            writer.WriteEndObject();
        }
    }
}

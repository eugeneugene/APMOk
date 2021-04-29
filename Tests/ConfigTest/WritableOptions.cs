using ConfigTest.JsonDocumentExtensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ConfigTest
{

    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IHostEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableOptions(
            IHostEnvironment environment,
            IOptionsMonitor<T> options,
            string section,
            string file)
        {
            _environment = environment;
            _options = options;
            _section = section;
            _file = file;
        }

        public WritableOptions()
        { }

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            using var document = JsonDocument.Parse(File.ReadAllText(physicalPath));
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Document is not an object");

            List<JsonProperty> collection = new();
            foreach (var item in root.EnumerateObject())
            {
                if (item.Name == _section)
                {
                    T s = JsonSerializer.Deserialize<T>(item.Value.GetString());
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

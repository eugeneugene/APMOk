using Microsoft.Extensions.Configuration;
using System;

namespace APMOkLib.CustomConfiguration
{
    public class UriParameterDecorator : IParameterDecorator<Uri>
    {
        private readonly Uri _defaultUri = null;

        public Uri DefaultValue => _defaultUri;

        public UriParameterDecorator()
        { }

        public UriParameterDecorator(Uri uri)
        {
            _defaultUri = uri;
        }

        public UriParameterDecorator(IConfigurationParameter<Uri> uriParameter)
        {
            _defaultUri = uriParameter.Value;
        }

        public Uri ExtractValue(IConfiguration configuration, string section)
        {
            if (configuration.GetSection(section).Exists())
            {
                var value = configuration.GetValue(section, string.Empty);
                if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                    return uri;
            }

            return _defaultUri;
        }
    }
}

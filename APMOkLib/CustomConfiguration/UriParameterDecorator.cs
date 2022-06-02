using Microsoft.Extensions.Configuration;
using System;

namespace APMOkLib.CustomConfiguration;

public class UriParameterDecorator : IParameterDecorator<Uri>
{
    private readonly Uri _defaultUri;

    public Uri DefaultValue => _defaultUri;

    public UriParameterDecorator(Uri? uri)
    {
        _defaultUri = uri ?? throw new ArgumentNullException(nameof(uri));
    }

    public UriParameterDecorator(IConfigurationParameter<Uri>? uriParameter)
    {
        if (uriParameter is null)
            throw new ArgumentNullException(nameof(uriParameter));

        _defaultUri = uriParameter.Value ?? throw new ArgumentException("Default Value cannot be null");
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

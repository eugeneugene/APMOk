using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace APMOkLib.CustomConfiguration;

/// <summary>
/// Фабрика параметров конфигурации
/// DependencyInjection Lifetime: Singleton
/// </summary>
public class ConfigurationParameterFactory
{
    private readonly ConcurrentDictionary<ISmartConfiguration, List<IConfigurationParameter>> _parameters = new();

    public IConfigurationParameter<T> CreateParameter<T>(ISmartConfiguration smartConfiguration, string name, string subSection, IParameterDecorator<T> defaultValueDecorator, string description)
    {
        var parameter = new ConfigurationParameter<T>(smartConfiguration, name, subSection, defaultValueDecorator, description);

        if (!_parameters.ContainsKey(smartConfiguration))
            _parameters[smartConfiguration] = new();

        _parameters[smartConfiguration].Add(parameter);

        return parameter;
    }

    public IReadOnlyCollection<ISmartConfiguration> SmartConfigurations => _parameters.Keys.AsReadOnly();

    public IReadOnlyCollection<IConfigurationParameter>? GetConfigurationParameters(ISmartConfiguration smartConfiguration)
    {
        if (smartConfiguration is null)
            throw new ArgumentNullException(nameof(smartConfiguration));

        if (!_parameters.ContainsKey(smartConfiguration))
            return null;

        return _parameters[smartConfiguration].AsReadOnly();
    }

    private class ConfigurationParameter<T> : IConfigurationParameter<T>
    {
        private readonly ISmartConfiguration _smartConfiguration;
        private readonly IParameterDecorator<T> _defaultValueDecorator;

        public string Name { get; }
        public string Section { get; }
        public string Description { get; }
        public T Value => _defaultValueDecorator.ExtractValue(_smartConfiguration.Configuration, Section);
        public string? StringValue => Value!.ToString();
        public T DefaultValue => _defaultValueDecorator.DefaultValue;
        public string? StringDefaultValue => DefaultValue!.ToString();
        public bool Exists => _smartConfiguration.Configuration.GetSection(Section).Exists();

        public ConfigurationParameter(ISmartConfiguration smartConfiguration, string name, string subSection, IParameterDecorator<T> defaultValueDecorator, string description)
        {
            _smartConfiguration = smartConfiguration;
            Name = name;
            Section = _smartConfiguration.SectionName + ":" + subSection;
            Description = description;
            _defaultValueDecorator = defaultValueDecorator ?? throw new ArgumentNullException(nameof(defaultValueDecorator));
        }
    }
}

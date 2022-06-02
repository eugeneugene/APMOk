using APMData;
using APMOkLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace APMOk.Models;

/// <summary>
/// APM Model
/// DI Lifetime: Singleton
/// </summary>
internal class APMOkModel : JsonToString, INotifyPropertyChanged
{
    private readonly IServiceProvider _scopeServiceProvider;

    private DisksReply? _systemDiskInfo;
    private PowerStateReply? _powerState;
    private bool _connectFailure;
    private ServiceVersionReply? _serviceVersion;

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ObservableConcurrentDictionary<string, APMValueProperty> APMValueDictionary = new();

    public APMOkModel(IServiceProvider scopeServiceProvider)
    {
        _scopeServiceProvider = scopeServiceProvider;
        _connectFailure = true;
    }

    public APMValueProperty? GetAPMValue(string? deviceID)
    {
        if (deviceID is not null && APMValueDictionary.ContainsKey(deviceID))
            return APMValueDictionary[deviceID];
        return null;
    }

    public void UpdateAPMValue(string deviceID, APMValueProperty apmValueProperty)
    {
        if (APMValueDictionary.ContainsKey(deviceID))
        {
            var value = APMValueDictionary[deviceID];
            if (!value.Equals(apmValueProperty))
            {
                value.OnMains = apmValueProperty.OnMains;
                value.OnBatteries = apmValueProperty.OnBatteries;
                value.Current = apmValueProperty.Current;
                Debug.WriteLine($"{deviceID}, Updated APMValueProperty: {value}");
                PropertyChanged?.Invoke(this, new(nameof(APMValueDictionary)));
            }
        }
        else
        {
            var value = new APMValueProperty(apmValueProperty.OnMains, apmValueProperty.OnBatteries, apmValueProperty.Current);
            APMValueDictionary[deviceID] = value;
            Debug.WriteLine($"{deviceID}, New APMValueProperty: {value}");
            PropertyChanged?.Invoke(this, new(nameof(APMValueDictionary)));
        }
    }

    public DisksReply? SystemDiskInfo
    {
        get => _systemDiskInfo;
        set
        {
            if (_systemDiskInfo is null || !_systemDiskInfo.Equals(value))
            {
                _systemDiskInfo = value;
                NotifyPropertyChanged(nameof(SystemDiskInfo));
            }
        }
    }

    public PowerStateReply? PowerState
    {
        get => _powerState;
        set
        {
            if (_powerState is null || !_powerState.Equals(value))
            {
                _powerState = value;
                NotifyPropertyChanged(nameof(PowerState));
            }
        }
    }

    public bool ConnectFailure
    {
        get => _connectFailure;
        set
        {
            if (_connectFailure != value)
            {
                _connectFailure = value;
                NotifyPropertyChanged(nameof(ConnectFailure));

                if (_connectFailure)
                    ServiceVersion = null;
                else
                    ObtainServiceVersion();
            }
        }
    }

    private void ObtainServiceVersion()
    {
        Task.Run(async () =>
        {
            ServiceVersion = await GetServiceVersionAsync();
        });
    }

    public ServiceVersionReply? ServiceVersion
    {
        get => _serviceVersion;
        set
        {
            if (_serviceVersion != value)
            {
                _serviceVersion = value;
                NotifyPropertyChanged(nameof(ServiceVersion));
            }
        }
    }

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
            return;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task<ServiceVersionReply> GetServiceVersionAsync()
    {
        Debug.WriteLine("GetServiceVersion");
        var _version = _scopeServiceProvider.GetRequiredService<Services.Version>();
        var _applicationLifetime = _scopeServiceProvider.GetRequiredService<IHostApplicationLifetime>();
        var reply = await _version.GetVersionAsync(_applicationLifetime.ApplicationStopping);
        Debug.WriteLine($"GetServiceVersion: {reply}");
        return reply;
    }
}

using APMOkSvc.Data;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class HostedService : IHostedService
    {
        private readonly DataContext _datacontext;
        private readonly DiskInfoService _diskInfoService;
        private readonly IHostApplicationLifetime _appLifetime;

        public HostedService( DataContext datacontext, DiskInfoService diskInfoService, IHostApplicationLifetime appLifetime)
        {
            _datacontext = datacontext;
            _diskInfoService = diskInfoService;
            _appLifetime = appLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var reply = _diskInfoService.EnumerateDisks();
            if (reply.ReplyResult == 0)
            {
                foreach (var disk in reply.DiskInfoEntries)
                {
                    if (_datacontext.ConfigDataSet.Any(item => item.Caption == disk.Caption))
                    {
                        var item = _datacontext.ConfigDataSet.Single(item => item.Caption == disk.Caption);
                        item.DefaultValue = disk.APMValue;
                    }
                    else
                    {
                        var item = new ConfigData
                        {
                            Caption = disk.Caption,
                            DefaultValue = disk.APMValue
                        };
                        _datacontext.ConfigDataSet.Add(item);
                    }
                }
                await _datacontext.SaveChangesAsync(_appLifetime.ApplicationStopping);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

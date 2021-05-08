using APMOkSvc.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class HostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DiskInfoServiceImpl _diskInfoServiceImpl;
        private readonly IHostApplicationLifetime _appLifetime;

        public HostedService(IServiceScopeFactory serviceScopeFactory, DiskInfoServiceImpl diskInfoServiceImpl, IHostApplicationLifetime appLifetime)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _diskInfoServiceImpl = diskInfoServiceImpl;
            _appLifetime = appLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var reply = _diskInfoServiceImpl.EnumerateDisks();
            if (reply.ReplyResult == 0)
            {
                using var serviceScope = _serviceScopeFactory.CreateScope();
                var db = serviceScope.ServiceProvider.GetService<DataContext>();

                foreach (var disk in reply.DiskInfoEntries.Where(item => item.InfoValid))
                {
                    if (db.ConfigDataSet.Any(item => item.Caption == disk.Caption))
                    {
                        var item = db.ConfigDataSet.Single(item => item.Caption == disk.Caption);
                        item.DefaultValue = disk.APMValue;
                    }
                    else
                    {
                        var item = new ConfigData
                        {
                            Caption = disk.Caption,
                            DefaultValue = disk.APMValue
                        };
                        db.ConfigDataSet.Add(item);
                    }
                }
                await db.SaveChangesAsync(_appLifetime.ApplicationStopping);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

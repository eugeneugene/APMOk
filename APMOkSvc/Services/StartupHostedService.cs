using APMData.Proto;
using APMOkSvc.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Startup Hosted Service
    /// DI Lifetime: Singleton (Hosted)
    /// </summary>
    public class StartupHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DiskInfoServiceImpl _diskInfoServiceImpl;
        private readonly IHostApplicationLifetime _appLifetime;

        public StartupHostedService(IServiceScopeFactory serviceScopeFactory, DiskInfoServiceImpl diskInfoServiceImpl, IHostApplicationLifetime appLifetime)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _diskInfoServiceImpl = diskInfoServiceImpl;
            _appLifetime = appLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //var reply = _diskInfoServiceImpl.EnumerateDisks();
            //if (reply.ReplyResult != 0)
            //{
            //    using var serviceScope = _serviceScopeFactory.CreateScope();
            //    var db = serviceScope.ServiceProvider.GetService<DataContext>();

            //    foreach (var disk in reply.DiskInfoEntries.Where(item => item.InfoValid))
            //    {
            //        var APMValueReply = _diskInfoServiceImpl.GetCurrentAPM(new CurrentAPMRequest { DeviceID = disk.DeviceID });
            //        if (db.ConfigDataSet.Any(item => item.DeviceID == disk.DeviceID))
            //        {
            //            var item = db.ConfigDataSet.Single(item => item.DeviceID == disk.DeviceID);
            //            item.DefaultValue = APMValueReply.APMValue;
            //            //db.ConfigDataSet.Update(item);
            //        }
            //        else
            //        {
            //            var item = new ConfigData
            //            {
            //                DeviceID = disk.DeviceID,
            //                DefaultValue = APMValueReply.APMValue
            //            };
            //            db.ConfigDataSet.Add(item);
            //        }
            //    }
            //    await db.SaveChangesAsync(_appLifetime.ApplicationStopping);
            //}
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

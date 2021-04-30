using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigTest
{
    class HostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IWritableOptions<APMConfiguration> _writableOptions;
        private readonly APMConfiguration _APMConfiguration;

        public HostedService(ILogger<HostedService> logger, IWritableOptions<APMConfiguration> APMConfiguration)
        {
            _logger = logger;
            _writableOptions = APMConfiguration;
            _APMConfiguration = APMConfiguration.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("APMConfiguration: {0}", _APMConfiguration);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _writableOptions.Update((changes) => { _ = changes.ConfigurationItems.Append(new APMConfigurationItem("Hello", 5)); });
            _logger.LogTrace("APMConfiguration: {0}", _APMConfiguration);
            return Task.CompletedTask;
        }
    }
}

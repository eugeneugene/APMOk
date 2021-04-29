using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Code
{
    public class StarterManager
    {
        private readonly Dictionary<PlatformID, IStarter> _starters = new() { };

        public StarterManager(params IStarter[] starters)
        {
            Add(starters);
        }

        public void Add(params IStarter[] starters)
        {
            if (starters == null)
                return;
            foreach (var starter in starters)
                _starters.Add(starter.Platform, starter);
        }

        public void Add(IStarter starter)
        {
            if (starter == null)
                return;
            _starters.Add(starter.Platform, starter);
        }

        public async Task<StarterRunResult> Start(ICollection<string> args)
        {
            if (!_starters.TryGetValue(Environment.OSVersion.Platform, out var starter))
            {
                Console.Error.WriteLine($"Платформа {Environment.OSVersion.Platform} не поддерживается");
                return StarterRunResult.Error;
            }
            var res = starter.ProcessCommandArgumens(args);
            switch (res)
            {
                case StarterArgumensResult.ExitError:
                    return StarterRunResult.Error;
                case StarterArgumensResult.ExitNoError:
                    return StarterRunResult.Success;
                case StarterArgumensResult.HelpNoError:
                    starter.ShowHelp();
                    return StarterRunResult.Success;
                case StarterArgumensResult.HelpError:
                    starter.ShowHelp();
                    return StarterRunResult.Error;
                case StarterArgumensResult.Run:
                    break;
                default:
                    throw new NotImplementedException("Неверная функция");
            }
            return await starter.ProcessHostRunAsync(CancellationToken.None);
        }
    }
}

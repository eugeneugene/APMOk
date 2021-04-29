using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace APMTest.Code
{
    public interface IStarter
    {
        PlatformID Platform { get; }
        StarterArgumensResult ProcessCommandArgumens(ICollection<string> args);
        Task<StarterRunResult> ProcessHostRunAsync(CancellationToken cancellationToken);
        void ShowHelp();
    }
}

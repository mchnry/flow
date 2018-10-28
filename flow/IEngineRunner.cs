using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow
{
    public interface IEngineRunner
    {
        Task<IEngineComplete> ExecuteAutoFinalizeAsync(string activityId, CancellationToken token);
        Task<IEngineFinalize> ExecuteAsync(string activityId, CancellationToken token);
    }
}

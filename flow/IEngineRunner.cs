using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow
{
    public interface IEngineRunner<TModel>
    {
        Task<IEngineComplete<TModel>> ExecuteAutoFinalizeAsync(CancellationToken token);
        Task<IEngineFinalize<TModel>> ExecuteAsync(CancellationToken token);
    }
}

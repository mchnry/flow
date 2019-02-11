using Mchnry.Core.Cache;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public interface IRuleEvaluator<TModel>
    {

        Task<bool> EvaluateAsync(IEngineScope<TModel> scope, LogicEngineTrace trace, CancellationToken token);

    }
}

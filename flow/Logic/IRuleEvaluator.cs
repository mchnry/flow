using Mchnry.Core.Cache;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow.Logic
{
    public interface IRuleEvaluator<TModel>
    {

        LogicDefine.Evaluator Definition { get; }
        
        Task EvaluateAsync(IEngineScope<TModel> scope, IEngineTrace trace, IRuleResult result, CancellationToken token);

    }
}

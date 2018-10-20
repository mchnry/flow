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
    public interface IRuleEvaluator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="context"></param>
        /// <param name="processId"></param>
        /// <param name="state"></param>
        /// <param name="validations"></param>
        /// <param name="expected"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> EvaluateAsync(Define.Rule definition, string processId, ICacheManager state, IValidationContainer validations, IStepTracer<string> tracer, CancellationToken token);

    }
}

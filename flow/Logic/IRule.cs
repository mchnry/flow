using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public interface IRule
    {
        Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token);

    }
}

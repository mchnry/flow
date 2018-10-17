using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public class Expression : IRule
    {
        public async Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}

using Mchnry.Flow.Analysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow
{
    public interface IEngineLinter<TModel>
    {
        Task<LintInspector> LintAsync(Action<Case> mockCase, CancellationToken token);
    }
}

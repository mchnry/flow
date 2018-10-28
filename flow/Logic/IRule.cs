using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal interface IRule
    {
        Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token);

    }
}

using Mchnry.Flow.Logic.Define;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal interface IRule<TModel>: IExpression
    {
        Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token);
        bool Inner { get; }
    }
}

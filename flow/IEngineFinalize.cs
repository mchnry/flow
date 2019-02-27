using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow
{
    public interface IEngineFinalize<TModel>
    {
        Task<IEngineComplete<TModel>> FinalizeAsync(CancellationToken token);
    }
}
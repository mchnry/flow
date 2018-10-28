using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow
{
    public interface IEngineFinalize
    {
        Task<IEngineComplete> FinalizeAsync(CancellationToken token);
    }
}
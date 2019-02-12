using Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    public interface IActionFactory
    {
        IAction<TModel> GetAction<TModel>(Define.ActionDefinition definition);
    }

    internal class NoActionFactory : IActionFactory {
        public IAction<TModel> GetAction<TModel>(ActionDefinition definition)
        {
            return default(IAction<TModel>);
        }
    }

}

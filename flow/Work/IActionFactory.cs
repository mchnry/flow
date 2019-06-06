using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    public interface IActionFactory
    {
        IAction<TModel> GetAction<TModel>(WorkDefine.ActionDefinition definition);
    }


}

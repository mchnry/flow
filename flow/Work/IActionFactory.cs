namespace Mchnry.Flow.Work
{
    public interface IActionFactory
    {
        IAction<TModel> GetAction<TModel>(Define.ActionDefinition definition);
    }
}

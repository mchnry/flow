namespace Mchnry.Flow.Work
{
    public interface IActionFactory
    {
        IAction GetAction(Define.ActionDefinition definition);
    }
}

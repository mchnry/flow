namespace Mchnry.Flow.Logic
{
    using Mchnry.Core.Cache;

    public interface IRuleEngine
    {
        IRuleEvaluator GetEvaluator(string id);

        bool? GetResult(Define.Rule definition);
        void SetResult(Define.Rule definition, bool result);
        string CurrentProcessId { get; }
        ICacheManager State { get; }

        IValidationContainer GetContainer(Define.Rule rule);
    }
}

namespace Mchnry.Flow.Logic
{
    using Mchnry.Core.Cache;

    public interface IRuleEngine
    {
        IRuleEvaluator GetEvaluator(Define.Evaluator definition);

        bool? GetResult(Define.Evaluator defintion, string context);
        void SetResult(Define.Evaluator definition, string context, bool result);
        string CurrentProcessId { get; }
        ICacheManager State { get; set; }

        IValidationContainer GetContainer(Define.Evaluator definition, string context);
    }
}

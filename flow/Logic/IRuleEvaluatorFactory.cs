namespace Mchnry.Flow.Logic
{
    public interface IRuleEvaluatorFactory
    {
        IRuleEvaluator<TModel> GetRuleEvaluator<TModel>(Define.Evaluator definition);
    }
}

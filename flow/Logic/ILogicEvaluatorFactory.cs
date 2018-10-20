namespace Mchnry.Flow.Logic
{
    public interface IRuleEvaluatorFactory
    {
        IRuleEvaluator GetRuleEvaluator(Define.Evaluator definition);
    }
}

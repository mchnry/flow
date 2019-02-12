using Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow.Logic
{
    public interface IRuleEvaluatorFactory
    {
        IRuleEvaluator<TModel> GetRuleEvaluator<TModel>(Define.Evaluator definition);
    }

    internal class NoRuleEvaluatorFactory : IRuleEvaluatorFactory
    {
        public IRuleEvaluator<TModel> GetRuleEvaluator<TModel>(Evaluator definition)
        {
            return default(IRuleEvaluator<TModel>);
        }
    }
}

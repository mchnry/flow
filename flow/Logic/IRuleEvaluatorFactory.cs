using Mchnry.Flow.Configuration;
using Mchnry.Flow.Logic.Define;
using System.Collections.Generic;
using LogicDefine = Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow.Logic
{
    public interface IRuleEvaluatorFactory
    {
        IRuleEvaluator<TModel> GetRuleEvaluator<TModel>(Define.Evaluator definition);
    }

    internal class ProxyEvaluatorFactory<TModel>
    {

        private Dictionary<string, IRuleEvaluator<TModel>> evaluators = new Dictionary<string, IRuleEvaluator<TModel>>();
        public Config Configuration { get; set; }

        public  ProxyEvaluatorFactory(Configuration.Config configuration) {
            this.Configuration = configuration;

            IRuleEvaluator<TModel> trueEvaluator = new AlwaysTrueEvaluator<TModel>();
            this.evaluators.Add(ConventionHelper.TrueEvaluator(this.Configuration.Convention), trueEvaluator);
        }

        public void AddEvaluator(string id, IRuleEvaluator<TModel> evaluator)
        {
            id = ConventionHelper.EnsureConvention(NamePrefixOptions.Evaluator, id, this.Configuration.Convention);
            if (!this.evaluators.ContainsKey(id))
            {
                this.evaluators.Add(id, evaluator);
            }
        }

        public IRuleEvaluatorFactory proxy { get; set; }

        public IRuleEvaluator<TModel> GetRuleEvaluator(Evaluator def)
        {
            LogicDefine.Evaluator withoutConvention = new LogicDefine.Evaluator()
            {
                Id = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention),
                Description = def.Description
            };
            IRuleEvaluator<TModel> toReturn = default(IRuleEvaluator<TModel>);


            if (!this.evaluators.ContainsKey(def.Id) && !this.evaluators.ContainsKey(withoutConvention.Id))
            {

                if (this.proxy != null)
                {
                    try
                    {
                        string searchId = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention);
                        toReturn = this.proxy.GetRuleEvaluator<TModel>(withoutConvention);
                        if (toReturn == null)
                        {
                            toReturn = this.proxy.GetRuleEvaluator<TModel>(def);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw new LoadEvaluatorException(def.Id, ex);
                    }

                    if (default(IRuleEvaluator<TModel>) == toReturn)
                    {
                        throw new LoadEvaluatorException(def.Id);
                    }

                    this.evaluators.Add(def.Id, toReturn);
                }
            }
            else
            {
                if (this.evaluators.ContainsKey(withoutConvention.Id))
                {
                    toReturn = this.evaluators[withoutConvention.Id];
                }

                else
                {
                    toReturn = this.evaluators[def.Id];
                }
            }
            return toReturn;
        }
    }
}

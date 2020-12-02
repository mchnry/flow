using Mchnry.Flow.Configuration;
using System.Collections.Generic;
using LogicDefine = Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow.Logic
{

    internal class ProxyEvaluatorFactory<TModel>
    {

        private Dictionary<string, IRuleEvaluatorX<TModel>> evaluators = new Dictionary<string, IRuleEvaluatorX<TModel>>();
        public Config Configuration { get; set; }

        public ProxyEvaluatorFactory(Configuration.Config configuration)
        {
            this.Configuration = configuration;

            IRuleEvaluatorX<TModel> trueEvaluator = new AlwaysTrueEvaluator<TModel>();
            this.evaluators.Add(ConventionHelper.TrueEvaluator(this.Configuration.Convention), trueEvaluator);
        }

        public void AddEvaluator(string id, IRuleEvaluatorX<TModel> evaluator)
        {
            id = ConventionHelper.EnsureConvention(NamePrefixOptions.Evaluator, id, this.Configuration.Convention);
            if (!this.evaluators.ContainsKey(id))
            {
                this.evaluators.Add(id, evaluator);
            }
        }



        public IRuleEvaluatorX<TModel> GetRuleEvaluator(LogicDefine.Evaluator def)
        {
            LogicDefine.Evaluator withoutConvention = new LogicDefine.Evaluator()
            {
                Id = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention),
                Description = def.Description
            };
            IRuleEvaluatorX<TModel> toReturn = default(IRuleEvaluatorX<TModel>);


    
                if (this.evaluators.ContainsKey(withoutConvention.Id))
                {
                    toReturn = this.evaluators[withoutConvention.Id];
                }

                else
                {
                    toReturn = this.evaluators[def.Id];
                }
            
            return toReturn;
        }
    }
}

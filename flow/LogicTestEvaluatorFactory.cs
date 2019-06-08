using Mchnry.Flow.Analysis;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mchnry.Flow.Work;
using Mchnry.Flow.Work.Define;
using Mchnry.Flow.Configuration;

namespace Mchnry.Flow
{
    internal class LogicTestEvaluatorFactory 
    {
        private readonly Configuration.Config configuration;

        public LogicTestEvaluatorFactory(Case testCase, Configuration.Config configuration)
        {
            this.TestCase = testCase;
            this.configuration = configuration;
        }

        public Case TestCase { get; }

        public IRuleEvaluator<TModel> GetRuleEvaluator<TModel>(Evaluator definition)
        {

            if (definition.Id == ConventionHelper.TrueEvaluator(this.configuration.Convention))
            {
                return new AlwaysTrueEvaluator<TModel>();
            } else {

                //in cases where the rule has context, then there will be a test case for 
                //each individual context.  in that case, pass all to the preset rule.
                List<Rule> toTest = this.TestCase.Rules.Where(r => r.Id == definition.Id).ToList();
                return new PreSetRuleEvaluator<TModel>(toTest);
            }
        }
    }

    internal class LogicTestActionFactory 
    {
        public IAction<TModel> GetAction<TModel>(ActionDefinition definition)
        {
            return new FakeAction<TModel>(definition);
        }
    }


}

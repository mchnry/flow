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
    internal class LogicTestEvaluatorFactory : IRuleEvaluatorFactory
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

                Rule toTest = this.TestCase.Rules.FirstOrDefault(r => r.Id == definition.Id);
                return new PreSetRuleEvaluator<TModel>(toTest.TrueCondition);
            }
        }
    }

    internal class LogicTestActionFactory : IActionFactory
    {
        public IAction<TModel> GetAction<TModel>(ActionDefinition definition)
        {
            return new FakeAction<TModel>(definition);
        }
    }

    
}

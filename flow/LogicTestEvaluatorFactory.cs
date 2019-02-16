using Mchnry.Flow.Analysis;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mchnry.Flow.Work;
using Mchnry.Flow.Work.Define;

namespace Mchnry.Flow
{
    internal class LogicTestEvaluatorFactory : IRuleEvaluatorFactory
    {
        public LogicTestEvaluatorFactory(Case testCase)
        {
            this.TestCase = testCase;
        }

        public Case TestCase { get; }

        public IRuleEvaluator<TModel> GetRuleEvaluator<TModel>(Evaluator definition)
        {

            if (definition.Id == "true")
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

using System;
using System.Collections.Generic;
using System.Text;
using Mchnry.Flow.Logic;
using Define = Mchnry.Flow.Logic.Define;
using Moq;
using Xunit;
using Mchnry.Flow;
using Mchnry.Flow.Work;

namespace Test.Lint
{
    public class LintTests
    {

        [Fact]
        public async void TestSimpleLint()
        {

            IBuilder<string> builder = Builder<string>.CreateBuilder("test");
            var builderWorkflow = builder.Build(a =>
            {
                a.IfThenDo(If => If.And(F => F.True("ev1"), S => S.And(F => F.True("ev2"), S2 => S2.True("ev3"))),
                    Then => Then.DoNothing());
            });

            Mock<IWorkflowDefinitionFactory> mkDefFactory = new Mock<IWorkflowDefinitionFactory>();
            mkDefFactory.Setup(g => g.GetWorkflow<string>(It.IsAny<string>())).Returns(builderWorkflow);

            IEngineLoader<string> e = Mchnry.Flow.Engine<string>.CreateEngine();
            e.SetWorkflowDefinitionFactory(mkDefFactory.Object);
            
            

            IEngineLinter<string> linter = e.Lint("test");
            await linter.LintAsync((l) => { }, null, new System.Threading.CancellationToken());

        }

        //[Fact]
        //public async void TestSimpleLintWithIntent()
        //{
        //    Mock<IRuleEvaluatorFactory> mkFactory = new Mock<IRuleEvaluatorFactory>();
        //    Mock<IRuleEvaluator<string>> mkEval = new Mock<IRuleEvaluator<string>>();

        //    mkFactory.Setup(g => g.GetRuleEvaluator<string>(It.IsAny<Define.Evaluator>())).Returns(mkEval.Object);

        //    List<Define.Evaluator> evals = new List<Define.Evaluator>()
        //    {
        //        new Define.Evaluator() { Id = "ev1"},
        //        new Define.Evaluator() { Id = "ev2"}
                
        //    };
        //    List<Define.Equation> eqs = new List<Define.Equation>()
        //    {
        //        new Define.Equation() { Id = "eq1.1", Condition = Operand.Or, First = "ev2|1", Second = "ev2|2" },
        //        new Define.Equation() { Id = "eq1", Condition = Operand.And, First = "ev1", Second = "eq1.1" }
        //    };

        //    IEngineLoader<string> engine = Mchnry.Flow.Engine<string>.CreateEngine(new Mchnry.Flow.Work.Define.Workflow() { Equations = eqs, Evaluators = evals })
        //        .SetEvaluatorFactory(mkFactory.Object);

        //    var tests = await engine.LintAsync((l) => { l.Intent("ev2").HasContext<int>().OneOfInclusive(); }, new System.Threading.CancellationToken());

        //    //there is only one root equation, so only one test case
        //    Assert.True(tests.LogicTests.Count == 1);
        //    //ev1 can be true or false.... ev2 is only one / inclusive, so there is a true for each context, and a case where niether are true.... so 6 casees
        //    //ev1   ev2|1   ev2|2
        //    //true  true    false
        //    //true  false   true
        //    //true  false   false
        //    //... same for ev1 = false
        //    Assert.True(tests.LogicTests[0].TestCases.Count == 6);

        //}

        //with intent, fails if context does not match expected type


    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using Mchnry.Flow.Logic;
using Define = Mchnry.Flow.Logic.Define;
using Moq;
using Xunit;


namespace Test.Lint
{
    public class LintTests
    {

        [Fact]
        public void TestSimpleLint()
        {
            Mock<IRuleEvaluatorFactory> mkFactory = new Mock<IRuleEvaluatorFactory>();
            Mock<IRuleEvaluator> mkEval = new Mock<IRuleEvaluator>();

            mkFactory.Setup(g => g.GetRuleEvaluator(It.IsAny<Define.Evaluator>())).Returns(mkEval.Object);

            List<Define.Evaluator> evals = new List<Define.Evaluator>()
            {
                new Define.Evaluator() { Id = "ev1"},
                new Define.Evaluator() { Id = "ev2"},
                new Define.Evaluator() { Id = "ev3"}
            };
            List<Define.Equation> eqs = new List<Define.Equation>()
            {
                new Define.Equation() { Id = "eq1.1", Condition = Operand.And, First = "ev2", Second = "ev3" },
                new Define.Equation() { Id = "eq1", Condition = Operand.And, First = "ev1", Second = "eq1.1" }
            };

            RuleEngine engine = new RuleEngine(mkFactory.Object, evals, eqs);

            engine.Lint((l) => { });

        }

        [Fact]
        public void TestSimpleLintWithIntent()
        {
            Mock<IRuleEvaluatorFactory> mkFactory = new Mock<IRuleEvaluatorFactory>();
            Mock<IRuleEvaluator> mkEval = new Mock<IRuleEvaluator>();

            mkFactory.Setup(g => g.GetRuleEvaluator(It.IsAny<Define.Evaluator>())).Returns(mkEval.Object);

            List<Define.Evaluator> evals = new List<Define.Evaluator>()
            {
                new Define.Evaluator() { Id = "ev1"},
                new Define.Evaluator() { Id = "ev2"}
                
            };
            List<Define.Equation> eqs = new List<Define.Equation>()
            {
                new Define.Equation() { Id = "eq1.1", Condition = Operand.Or, First = "ev2|1", Second = "ev2|2" },
                new Define.Equation() { Id = "eq1", Condition = Operand.And, First = "ev1", Second = "eq1.1" }
            };

            RuleEngine engine = new RuleEngine(mkFactory.Object, evals, eqs);

            engine.Lint((l) => { l.Intent("ev2").HasContext<int>(); });

        }

        //with intent, fails if context does not match expected type


    }
}
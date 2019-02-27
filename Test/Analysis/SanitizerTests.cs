using Mchnry.Flow.Analysis;
using Mchnry.Flow.Configuration;
using Mchnry.Flow.Diagnostics;
using System.Collections.Generic;
using Xunit;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using System.Linq;

namespace Test.Analysis
{
    public class SanitizerTests
    {

        Config defaultConfig = new Config();

        //true added to evaluators
        [Fact]
        public void EvaluatorInferredAndAdded()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Evaluators.Count(g => g.Id == "evaluator.test"));

        }
        //placeholder added to actions
        //[Fact]
        //public void PlaceHolderActionAdded()
        //{
        //    WorkDefine.Workflow testWF = new WorkDefine.Workflow()
        //    {
        //        Activities = new List<WorkDefine.Activity>()
        //        {
        //            new WorkDefine.Activity()
        //            {
        //                Id = "activity.test",
        //                Reactions = new List<WorkDefine.Reaction>()
        //                {
        //                    new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" }
        //                }
        //            }
        //        }
        //    };
        //    StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
        //    tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
        //    Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
        //    WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
        //    Assert.Equal(1, sanitized.Actions.Count(g => g.Id == "*placeHolder"));
        //}
        //evaluator added only once
        [Fact]
        public void InferredEvaluatorAddedOnlyOnce()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Evaluators.Count(g => g.Id == "evaluator.test"));
        }
        //action added only once
        [Fact]
        public void InferredActionAddedOnlyOnce()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Actions.Count(g => g.Id == "action.reaction"));
        }
        //equation added only once
        [Fact]
        public void InferredEquationAddedOnlyOnce()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Equations.Count(g => g.Id == "equation.test"));
        }
        //inferred activity added only once
        //[Fact]
        //public void InferredActivityAddedOnlyOnce()
        //{
        //    WorkDefine.Workflow testWF = new WorkDefine.Workflow()
        //    {
        //        Activities = new List<WorkDefine.Activity>()
        //        {
        //            new WorkDefine.Activity()
        //            {
        //                Action = "action.test",
        //                Id = "activity.test",
        //                Reactions = new List<WorkDefine.Reaction>()
        //                {
        //                    new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
        //                    new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" }
        //                }
        //            }
        //        }
        //    };
        //    StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
        //    tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
        //    Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
        //    WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
        //    Assert.Equal(1, sanitized.Activities.Count(g => g.Id == "activity.reaction"));
        //}
        //negation equation added if negate evaluator
        [Fact]
        public void InferredNegatedEvaluatorEquationAdded()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
                            new WorkDefine.Reaction() { Logic = "!evaluator.test", Work ="action.badreaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Equations.Count(g => g.Id == "equation.NOT.test"));
        }
        //negation equation added only once if negate evaluator
        [Fact]
        public void InferredNegatedEvaluatorEquationAddedOnlyOnce()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
                            new WorkDefine.Reaction() { Logic = "!evaluator.test", Work ="action.badreaction" },
                            new WorkDefine.Reaction() { Logic = "!evaluator.test", Work ="action.anotherbadreaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Equations.Count(g => g.Id == "equation.NOT.test"));
        }
        //negation equation added if negate equation
        [Fact]
        public void InferredNegatedEquationAdded()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Equations = new List<LogicDefine.Equation>()
                {
                    new LogicDefine.Equation() { Condition = Mchnry.Flow.Logic.Operand.And, First = "evaluator.first", Id = "equation.testeq"}
                },
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
                            new WorkDefine.Reaction() { Logic = "!evaluator.test", Work ="action.badreaction" },
                            new WorkDefine.Reaction() { Logic = "!equation.testeq", Work ="action.anotherbadreaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Equations.Count(g => g.Id == "equation.NOT.testeq"));
        }
        //negation equation addded only once if negate equation
        [Fact]
        public void InferredNegatedEquationAddedOnlyOnce()
        {
            WorkDefine.Workflow testWF = new WorkDefine.Workflow("test")
            {
                Equations = new List<LogicDefine.Equation>()
                {
                    new LogicDefine.Equation() { Condition = Mchnry.Flow.Logic.Operand.And, First = "evaluator.first", Id = "equation.testeq"}
                },
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity()
                    {
                        //Action = "action.test",
                        Id = "activity.test",
                        Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "evaluator.test", Work ="action.reaction" },
                            new WorkDefine.Reaction() { Logic = "!equation.testeq", Work ="action.badreaction" },
                            new WorkDefine.Reaction() { Logic = "!equation.testeq", Work ="action.anotherbadreaction" }
                        }
                    }
                }
            };
            StepTracer<LintTrace> tracer = new StepTracer<LintTrace>();
            tracer.TraceFirst(new LintTrace(LintStatusOptions.Sanitizing, "Testing Sanitizer"));
            Sanitizer toTest = new Sanitizer(tracer, this.defaultConfig);
            WorkDefine.Workflow sanitized = toTest.Sanitize(testWF);
            Assert.Equal(1, sanitized.Equations.Count(g => g.Id == "equation.NOT.testeq"));
        }



    }
}

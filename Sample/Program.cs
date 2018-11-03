using System;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Sample
{
    class EvalProfanity : IRuleEvaluator
    {
        public async Task<bool> EvaluateAsync(IEngineScope scope, LogicEngineTrace trace, CancellationToken token)
        {

            return true;
        }
    }
    class WritePost : IAction
    {
        public async Task<bool> CompleteAsync(IEngineScope scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            Console.WriteLine("Write Post");
            return true;
        }
    }

    class ActionFactory : IActionFactory
    {
        public IAction GetAction(WorkDefine.ActionDefinition definition)
        {
            return new WritePost();
        }
    }

    class RuleEvaluatorFactory : IRuleEvaluatorFactory
    {
        public IRuleEvaluator GetRuleEvaluator(LogicDefine.Evaluator definition)
        {
            return new EvalProfanity();
        }
    }

    class Program
    {


        public static async Task Main(string[] args)
        {

            WorkDefine.Workflow sampleWorkflow = new WorkDefine.Workflow()
            {
                Evaluators = new System.Collections.Generic.List<LogicDefine.Evaluator>()
                {
                    new LogicDefine.Evaluator() { Id = "evalProfanity"}
                },
                Equations = new System.Collections.Generic.List<LogicDefine.Equation>()
                {
                    new LogicDefine.Equation() { Condition = Mchnry.Flow.Logic.Operand.And, First = "evalProfanity", Id = "toCompletePost" }
                },
                Actions = new System.Collections.Generic.List<WorkDefine.ActionDefinition>()
                {
                    new WorkDefine.ActionDefinition() { Id = "WritePost"}
                },
                Activities = new System.Collections.Generic.List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity() { Id = "CompletePost", Action = "WritePost" },
                    new WorkDefine.Activity() {
                        Id = "PostMessage",
                        //notice that there is no action referenced... engine will inject a placeholder
                        Reactions = new System.Collections.Generic.List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { EquationId = "evalProfanity", ActivityId = "CompletePost" }
                        }
                    }
                }
            };

            var engine = Engine.CreateEngine(sampleWorkflow).SetActionFactory(new ActionFactory())
                .SetEvaluatorFactory(new RuleEvaluatorFactory())
                .Start();
            await engine.ExecuteAsync("PostMessage", new CancellationToken());

           

        }
    }
}

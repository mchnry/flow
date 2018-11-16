using System;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using Newtonsoft.Json;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Sample
{

    class Post
    {
        public string message { get; set; }
    }

    class EvalProfanity : IRuleEvaluator
    {
        public async Task<bool> EvaluateAsync(IEngineScope scope, LogicEngineTrace trace, CancellationToken token)
        {
            Post model = scope.GetModel<Post>("model");
            return await Task.FromResult<bool>(model.message.IndexOf("badword") > -1);
            
        }
    }

    class EvalMultiOffender : IRuleEvaluator
    {
        public async Task<bool> EvaluateAsync(IEngineScope scope, LogicEngineTrace trace, CancellationToken token)
        {
            return await Task.FromResult<bool>(true);
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

    class SuspendUser : IAction
    {
        public async Task<bool> CompleteAsync(IEngineScope scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            scope.Defer(new SuspendUserDefer(), false);

            
            return true;
        }
    }
    class SuspendUserDefer : IDeferredAction
    {
        public string Id => "deferSuspend";

        public async Task<bool> CompleteAsync(IEngineScope scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            Console.WriteLine("User Suspended");
            return true;
        }
    }


    class ActionFactory : IActionFactory
    {
        public IAction GetAction(WorkDefine.ActionDefinition definition)
        {
            switch (definition.Id)
            {
                case "WritePost": return new WritePost();
                case "SuspendUser": return new SuspendUser();
                default: return null;
            }
        }
    }

    class RuleEvaluatorFactory : IRuleEvaluatorFactory
    {
        public IRuleEvaluator GetRuleEvaluator(LogicDefine.Evaluator definition)
        {
            switch (definition.Id)
            {
                case "evalProfanity": return new EvalProfanity();
                case "evalMultiOffender": return new EvalMultiOffender();
                default: return null;
            }
        }
    }

    class Program
    {


        public static async Task Main(string[] args)
        {

            WorkDefine.Workflow sampleWorkflow = new WorkDefine.Workflow()
            {
                //Evaluators = new System.Collections.Generic.List<LogicDefine.Evaluator>()
                //{
                //    new LogicDefine.Evaluator() { Id = "evalProfanity" },
                //    new LogicDefine.Evaluator() { Id = "evalMultiOffender" }
                //},
                ////Equations = new System.Collections.Generic.List<LogicDefine.Equation>()
                ////{
                ////    new LogicDefine.Equation() { Condition = Mchnry.Flow.Logic.Operand.And, First = "evalProfanity", Id = "toCompletePost" }
                ////},
                //Actions = new System.Collections.Generic.List<WorkDefine.ActionDefinition>()
                //{
                //    new WorkDefine.ActionDefinition() { Id = "WritePost"},
                //    new WorkDefine.ActionDefinition() { Id = "SuspendUser"}
                //},
                Activities = new System.Collections.Generic.List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity() {
                        Id = "CompletePost",
                        Action = "WritePost",
                        Reactions = new System.Collections.Generic.List<WorkDefine.Reaction>() {
                            new WorkDefine.Reaction() { EquationId = "evalMultiOffender", ActivityId = "SuspendUser" }
                        }
                    },
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
                .SetEvaluatorFactory(new RuleEvaluatorFactory()).SetModel<Post>("model", new Post() { message = "badword" })
                .Start();

            IEngineFinalize f = null;
            try
            {
                f = await engine.ExecuteAsync("PostMessage", new CancellationToken());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var c = await f.FinalizeAsync(new CancellationToken());

            string s = JsonConvert.SerializeObject(c.Process, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore } );

            Console.WriteLine(s);

            Console.ReadLine();

        }
    }
}

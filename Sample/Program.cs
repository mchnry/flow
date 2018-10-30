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
        public Task<bool> EvaluateAsync(IEngineScope scope, LogicEngineTrace trace, CancellationToken token)
        {
            
            throw new System.NotImplementedException();
        }
    }
    class WritePost: IAction
    {

    }

    class Program
    {
        static void Main(string[] args)
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


        }
    }
}

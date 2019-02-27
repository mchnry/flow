using Mchnry.Flow;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Sample
{

    public class Foo
    {
        public int Id { get; set; }
    }
    public class Bar
    {
        public int Id { get; set; }
    }

    public class DoSomethingAction : IAction<Bar>
    {
        async Task<bool> IAction<Bar>.CompleteAsync(IEngineScope<Bar> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            Console.WriteLine("foo");
            return await Task.FromResult<bool>(true);
        }
    }
    public class RunAnotherWorkflowAction : IAction<Foo>
    {
        async Task<bool> IAction<Foo>.CompleteAsync(IEngineScope<Foo> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            await scope.RunWorkflowAsync<Bar>("second", new Bar(), token);
            return await Task.FromResult<bool>(true);
        }
    }
    public class AIsTrueEvaluator : IRuleEvaluator<Foo>
    {
        async Task IRuleEvaluator<Foo>.EvaluateAsync(IEngineScope<Foo> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }
    public class BIsTrueEvaluator : IRuleEvaluator<Foo>
    {
        async Task IRuleEvaluator<Foo>.EvaluateAsync(IEngineScope<Foo> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }
    public class CIsTrueEvaluator : IRuleEvaluator<Bar>
    {
        async Task IRuleEvaluator<Bar>.EvaluateAsync(IEngineScope<Bar> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }
    public class DIsTrueEvaluator : IRuleEvaluator<Bar>
    {
        async Task IRuleEvaluator<Bar>.EvaluateAsync(IEngineScope<Bar> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }

    public class ActionFactory : IActionFactory
    {
        IAction<TModel> IActionFactory.GetAction<TModel>(WorkDefine.ActionDefinition definition)
        {
            IAction<TModel> toReturn = default(IAction<TModel>);
            switch (definition.Id)
            {
                case "runAnotherWorkflow":
                    toReturn = (IAction<TModel>)new RunAnotherWorkflowAction();
                    break;
                case "doSomething":
                    toReturn = (IAction<TModel>)new DoSomethingAction();
                    break;
            }
            return toReturn;
        }
    }
    public class EvaluatorFactory : IRuleEvaluatorFactory
    {
        IRuleEvaluator<TModel> IRuleEvaluatorFactory.GetRuleEvaluator<TModel>(LogicDefine.Evaluator definition)
        {
            IRuleEvaluator<TModel> toReturn = default(IRuleEvaluator<TModel>);

            switch(definition.Id)
            {
                case "aistrue":
                    toReturn = (IRuleEvaluator<TModel>)new AIsTrueEvaluator();
                    break;
                case "bistrue":
                    toReturn = (IRuleEvaluator<TModel>)new BIsTrueEvaluator();
                    break;
                case "cistrue":
                    toReturn = (IRuleEvaluator<TModel>)new CIsTrueEvaluator();
                    break;
                case "distrue":
                    toReturn = (IRuleEvaluator<TModel>)new DIsTrueEvaluator();
                    break;
            }

            return toReturn;
        }
    }

    public class WorkflowDefinitionFactory : IWorkflowDefinitionFactory
    {
        public WorkDefine.Workflow GetWorkflow(string workflowId)
        {
            WorkDefine.Workflow toReturn = null;
            switch (workflowId)
            {
                case "first":
                    toReturn = Builder.CreateBuilder("first").Build(ToDo => ToDo
                        .IfThenDo(
                            If => If.And(
                                First => First.True("aistrue"), Second => Second.True("bistrue")
                            ),
                            Then => Then.Do("runAnotherWorkflow")
                            )
                                
                    );
                    break;
                case "second":
                    toReturn = Builder.CreateBuilder("second").Build(ToDo => ToDo
                        .IfThenDo(
                            If => If.And(
                                First => First.True("cistrue"), Second => Second.True("distrue")
                            ),
                            Then => Then.Do("doSomething")
                            )

                    );
                    break;

            }

            return toReturn;
        }
    }

    class Program
    {


        public static async Task Main(string[] args)
        {

            var builder = Engine<Foo>.CreateEngine();
            var runner = builder.SetActionFactory(new ActionFactory())
                .SetEvaluatorFactory(new EvaluatorFactory())
                .SetWorkflowDefinitionFactory(new WorkflowDefinitionFactory())
                .Start("first", new Foo());

            var complete = runner.ExecuteAutoFinalizeAsync(new CancellationToken());





        }

    }


}


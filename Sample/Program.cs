using Mchnry.Core.JWT;
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
        public WorkDefine.ActionDefinition Definition => new WorkDefine.ActionDefinition()
        {
            Id = "doSomeThing",
            Description = "Does Something"
        };


        async Task<bool> IAction<Bar>.CompleteAsync(IEngineScope<Bar> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            Console.WriteLine(scope.CurrentActivity.Id);
            return await Task.FromResult<bool>(true);
        }
    }
    public class RunAnotherWorkflowAction : IAction<Foo>
    {
        public WorkDefine.ActionDefinition Definition => new WorkDefine.ActionDefinition()
        {
            Id = "anotherWorkflow",
            Description = "Run Another Workflow"
        };

        async Task<bool> IAction<Foo>.CompleteAsync(IEngineScope<Foo> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            await scope.RunWorkflowAsync<Bar>("second", new Bar(), token);
            return await Task.FromResult<bool>(true);
        }
    }
    public class AIsTrueEvaluator : IRuleEvaluator<Foo>
    {
        public LogicDefine.Evaluator Definition => new LogicDefine.Evaluator()
        {
            Id = "aIsTrue",
            Description = "Determines if A is true"
        };

        async Task IRuleEvaluator<Foo>.EvaluateAsync(IEngineScope<Foo> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }
    public class BIsTrueEvaluator : IRuleEvaluator<Foo>
    {

        public LogicDefine.Evaluator Definition => new LogicDefine.Evaluator()
        {
            Id = "bIsTrue",
            Description = "Determines if B is true"
        };

        async Task IRuleEvaluator<Foo>.EvaluateAsync(IEngineScope<Foo> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }
    public class CIsTrueEvaluator : IRuleEvaluator<Bar>
    {

        public LogicDefine.Evaluator Definition => new LogicDefine.Evaluator()
        {
            Id = "cIsTrue",
            Description = "Determines if C is true"
        };

        async Task IRuleEvaluator<Bar>.EvaluateAsync(IEngineScope<Bar> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }
    public class DIsTrueEvaluator : IRuleEvaluator<Bar>
    {

        public LogicDefine.Evaluator Definition => new LogicDefine.Evaluator()
        {
            Id = "dIsTrue",
            Description = "Determines if D is true"
        };
        async Task IRuleEvaluator<Bar>.EvaluateAsync(IEngineScope<Bar> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }

    //public class ActionFactory : IActionFactory
    //{
    //    IAction<TModel> IActionFactory.GetAction<TModel>(WorkDefine.ActionDefinition definition)
    //    {
    //        IAction<TModel> toReturn = default(IAction<TModel>);
    //        switch (definition.Id)
    //        {
    //            case "runAnotherWorkflow":
    //                toReturn = (IAction<TModel>)new RunAnotherWorkflowAction();
    //                break;
    //            case "doSomething":
    //                toReturn = (IAction<TModel>)new DoSomethingAction();
    //                break;
    //        }
    //        return toReturn;
    //    }
    //}
    //public class EvaluatorFactory : IRuleEvaluatorFactory
    //{
    //    IRuleEvaluator<TModel> IRuleEvaluatorFactory.GetRuleEvaluator<TModel>(LogicDefine.Evaluator definition)
    //    {
    //        IRuleEvaluator<TModel> toReturn = default(IRuleEvaluator<TModel>);

    //        switch (definition.Id)
    //        {
    //            case "aistrue":
    //                toReturn = (IRuleEvaluator<TModel>)new AIsTrueEvaluator();
    //                break;
    //            case "bistrue":
    //                toReturn = (IRuleEvaluator<TModel>)new BIsTrueEvaluator();
    //                break;
    //            case "cistrue":
    //                toReturn = (IRuleEvaluator<TModel>)new CIsTrueEvaluator();
    //                break;
    //            case "distrue":
    //                toReturn = (IRuleEvaluator<TModel>)new DIsTrueEvaluator();
    //                break;
    //        }

    //        return toReturn;
    //    }
    //}

    public class WorkflowBuilderFactory : IWorkflowBuilderFactory
    {
        public IBuilderWorkflow<T> GetWorkflow<T>(string workflowId)
        {
            IBuilderWorkflow<T> toReturn = default;
            ExpressionRef xRef = default;
            switch (workflowId)
            {
                case "first":
                    toReturn = (Builder<T>)Builder<Foo>.CreateBuilder("first").BuildFluent(ToDo => ToDo
                        .IfThenDo(
                            (If) => {
                                xRef = If.And(
                                    First => First.True((b) => b.Eval(new AIsTrueEvaluator()).IsTrue()), Second => Second.True((b) => b.Eval(new BIsTrueEvaluator()).IsTrue())
                                );
                            },
                            Then => Then.Do((a) => a.Do(new RunAnotherWorkflowAction()))
                            ).IfThenDo(If => If.True(xRef.Negate()), Then => Then.DoNothing())
                    );
                    break;
                case "second":
                    toReturn = (Builder<T>)Builder<Bar>.CreateBuilder("second").BuildFluent(ToDo => ToDo
                        .IfThenDo(
                            If => If.And(
                                First => First.True((a) => a.Eval(new CIsTrueEvaluator()).IsTrue()), Second => Second.True((a) => a.Eval(new DIsTrueEvaluator()).IsTrue())
                            ),
                            Then => Then.Do((a) => a.Do(new DoSomethingAction()))
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
            var runner = builder
                //.SetActionFactory(new ActionFactory())
                //.SetEvaluatorFactory(new EvaluatorFactory())
                .SetWorkflowDefinitionFactory(new WorkflowBuilderFactory())
                .Start("first", new Foo());

            var complete = runner.ExecuteAutoFinalizeAsync(new CancellationToken());


            Console.ReadLine();

            //RSAProv prov = new RSAProv(System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint, "5d6c96212ec044b31eff0f563644ece2bc968b3b");
            //var cert = prov.GetKey();

            //JWTHelper helper = new Mchnry.Core.JWT.JWTHelper();
            //var jwt = helper.Encode<ApiHeader, ApiToken>(new jwt<ApiHeader, ApiToken>()
            //{
            //    Header = new ApiHeader() { Algorithm = "HS384", exp = helper.DateToInt(TimeSpan.FromMinutes(1))[1], TokenName = "tkn" },
            //    Token = new ApiToken() { exp = helper.DateToInt(TimeSpan.FromMinutes(1))[1], iat = helper.DateToInt(TimeSpan.MinValue)[0], Subject = "jamie", JTI = "asdfasdf" }
            //}, cert);

            //bool expired;
            //var decoded = helper.Decode<ApiHeader, ApiToken>(jwt, cert, out expired);

        }

    }


}


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



    public class ActionFactory
    {
        public IAction<Foo> CtxAction { get => new CtxAction(); }
    }

    internal class CtxAction : IAction<Foo>
    {
        public WorkDefine.ActionDefinition Definition => new WorkDefine.ActionDefinition()
        {
            Description = "context action",
            Id = "ctxaction"
        };

        public async Task<bool> CompleteAsync(IEngineScope<Foo> scope, IEngineTrace trace, CancellationToken token)
        {
            Console.WriteLine($"context {scope.CurrentAction.Context}");
            return true;
        }
    }

    public class EvaluatorFactory
    {
        public IRuleEvaluator<Foo> CtxRule => new CtxRule();
    }

    internal class CtxRule : IRuleEvaluator<Foo>
    {
        public LogicDefine.Evaluator Definition => new LogicDefine.Evaluator()
        {
            Id = "ctxrule",
            Description = "context rule"
        };

        public async Task EvaluateAsync(IEngineScope<Foo> scope, IEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            Console.WriteLine($"evaluating with context {scope.CurrentRuleDefinition.Context}");
            result.Pass();
        }
    }

    public class WorkflowBuilderFactory : IWorkflowBuilderFactory
    {

        ActionFactory AF = new ActionFactory();
        EvaluatorFactory EF = new EvaluatorFactory();

        public IWorkflowBuilder<T> GetWorkflow<T>(string workflowId)
        {


    
            
            
            IBuilderWorkflow<T> toReturn = default;
            
            switch (workflowId)
            {
                case "first":
                    toReturn =  (IBuilderWorkflow<T>)Builder<Foo>.CreateBuilder("first").BuildFluent(ToDo => ToDo
                        .IfThenDo(
                            If => If.Rule(rule => rule.EvalWithContext(EF.CtxRule, "abc")),
                            Then => Then.Do(Do => Do.DoWithContext(AF.CtxAction, "123"))
                            )

                    );
                    string s = JsonConvert.SerializeObject(toReturn.Workflow, new JsonSerializerSettings() { Formatting = Formatting.Indented });
                    Console.WriteLine(s);
                    break;

            }

            return new WorkflowBuilder<T>( toReturn);
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


using Mchnry.Core.JWT;
using Mchnry.Flow;
using Mchnry.Flow.Configuration;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Sample
{


    public class ActionFactory
    {
        public IAction<string> DoIt => new DoIt();
    }

    internal class DoIt : IAction<string>
    {
        public async Task<bool> CompleteAsync(IEngineScope<string> scope, IEngineTrace trace, CancellationToken token)
        {
            Console.WriteLine("did it");
            return true;
        }
    }

    public class EvaluatorFactory
    {
        public IRuleEvaluator<string> ShouldIDoIt => new ShouldIDoIt();
    }

    [ArticulateOptions("Test If I Should Do It")]
    internal class ShouldIDoIt : IRuleEvaluator<string>
    {
        public async Task EvaluateAsync(IEngineScope<string> scope, IEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            result.Pass();
        }
    }

    public class BuilderFactory
    {
        private readonly ActionFactory af;
        private readonly EvaluatorFactory ef;

        public BuilderFactory(ActionFactory af, EvaluatorFactory ef)
        {
            this.af = af;
            this.ef = ef;
        }
        public IWorkflowBuilder<string> Workflow =>
            new WorkflowBuilder<string>(Builder<string>.CreateBuilder("workflow").BuildFluent(ToDo => ToDo
            .IfThenDo(
                If => If.Rule(rule => rule.Eval(ef.ShouldIDoIt).IsTrue()),
                Then => Then.Do(Do => Do.Do(af.DoIt))
            )));
    }



    class Program
    {



        public static async Task Main(string[] args)
        {
            
            ActionFactory AF = new ActionFactory();
            EvaluatorFactory EF = new EvaluatorFactory();
            BuilderFactory BF = new BuilderFactory(AF, EF);

            var engine = Engine<string>.CreateEngine();
            var runner = engine.StartFluent(BF.Workflow, "hello");

            var complete = await runner.ExecuteAutoFinalizeAsync(new CancellationToken());

            Console.ReadLine();

            /*
            UserHasPermission
            ItemInInventory


            */
        }

    }


}


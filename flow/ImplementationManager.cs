using Mchnry.Flow.Analysis;
using Mchnry.Flow.Configuration;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow
{

    internal interface IImplementationManager<TModel>
    {

        IAction<TModel> GetAction(WorkDefine.ActionDefinition id);
        IRuleEvaluator<TModel> GetEvaluator(LogicDefine.Evaluator id);
        WorkDefine.Workflow GetWorkflow(string id);

        IActionFactory ActionFactory { get; }
        IWorkflowDefinitionFactory DefinitionFactory { get; }
        IRuleEvaluatorFactory EvaluatorFactory { get; }

    }
    internal class ImplementationManager<TModel> : IImplementationManager<TModel>
    {


        //store reference to all factory created actions
        private Dictionary<string, IAction<TModel>> actions = new Dictionary<string, IAction<TModel>>();
        //store references to all factory created evaluators
        private Dictionary<string, IRuleEvaluator<TModel>> evaluators = new Dictionary<string, IRuleEvaluator<TModel>>();

        public IActionFactory ActionFactory { get; set; }
        public IRuleEvaluatorFactory EvaluatorFactory { get; set; }
        public IWorkflowDefinitionFactory DefinitionFactory { get; set; }

        public Config Configuration { get; }

        internal ImplementationManager()
        {
            this.ActionFactory = new NoActionFactory();
            this.EvaluatorFactory = new NoRuleEvaluatorFactory();
            this.DefinitionFactory = new NoWorkflowDefinitionFactory();

            this.Configuration = new Config();

            IRuleEvaluator<TModel> trueEvaluator = new AlwaysTrueEvaluator<TModel>();
            this.evaluators.Add(ConventionHelper.TrueEvaluator(this.Configuration.Convention), trueEvaluator);
        }

        internal ImplementationManager(Configuration.Config configuration) : this()
        {

            this.Configuration = configuration;
        }

        internal ImplementationManager(IActionFactory actionFactory, IRuleEvaluatorFactory evaluatorFactory, IWorkflowDefinitionFactory definitionFactory, Configuration.Config configuration) : this(configuration)
        {
            this.ActionFactory = actionFactory;
            this.EvaluatorFactory = evaluatorFactory;
            this.DefinitionFactory = definitionFactory;

        }

        public virtual IAction<TModel> GetAction(WorkDefine.ActionDefinition def)
        {
            WorkDefine.ActionDefinition withoutConvention = new WorkDefine.ActionDefinition()
            {
                Id = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention),
                Description = def.Description
            };

            IAction<TModel> toReturn = default(IAction<TModel>);
            if (!this.actions.ContainsKey(def.Id) && !this.actions.ContainsKey(withoutConvention.Id))
            {



                try
                {


                    toReturn = this.ActionFactory.GetAction<TModel>(withoutConvention);
                    //try with convention
                    if (toReturn == null)
                    {
                        toReturn = this.ActionFactory.GetAction<TModel>(def);
                    }
                }
                catch (System.Exception ex)
                {
                    throw new LoadActionException(def.Id, ex);
                }

                if (default(IAction<TModel>) == toReturn)
                {
                    throw new LoadActionException(def.Id);
                }

                this.actions.Add(def.Id, toReturn);

            }
            else
            {
                if (this.actions.ContainsKey(withoutConvention.Id))
                {
                    toReturn = this.actions[withoutConvention.Id];
                }
                else
                {
                    toReturn = this.actions[def.Id];
                }
            }
            return toReturn;
        }

        public virtual WorkDefine.Workflow GetWorkflow(string id)
        {
            return this.DefinitionFactory.GetWorkflow(id);
        }

        public virtual IRuleEvaluator<TModel> GetEvaluator(LogicDefine.Evaluator def)
        {

            LogicDefine.Evaluator withoutConvention = new LogicDefine.Evaluator()
            {
                Id = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention),
                Description = def.Description
            };
            IRuleEvaluator<TModel> toReturn = default(IRuleEvaluator<TModel>);


            if (!this.evaluators.ContainsKey(def.Id) && !this.evaluators.ContainsKey(withoutConvention.Id))
            {

                try
                {
                    string searchId = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention);
                    toReturn = this.EvaluatorFactory.GetRuleEvaluator<TModel>(withoutConvention);
                    if (toReturn == null)
                    {
                        toReturn = this.EvaluatorFactory.GetRuleEvaluator<TModel>(def);
                    }
                }
                catch (System.Exception ex)
                {
                    throw new LoadEvaluatorException(def.Id, ex);
                }

                if (default(IRuleEvaluator<TModel>) == toReturn)
                {
                    throw new LoadEvaluatorException(def.Id);
                }

                this.evaluators.Add(def.Id, toReturn);
            }
            else
            {
                if (this.evaluators.ContainsKey(withoutConvention.Id))
                {
                    toReturn = this.evaluators[withoutConvention.Id];
                }

                else
                {
                    toReturn = this.evaluators[def.Id];
                }
            }
            return toReturn;
        }

        internal virtual void AddEvaluator(string id, Func<IEngineScope<TModel>, LogicEngineTrace, IRuleResult, CancellationToken, Task> evaluator)
        {
            this.evaluators.Add(id, new DynamicEvaluator<TModel>(evaluator));

        }
        internal virtual void AddAction(string id, Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action)
        {

            this.actions.Add(id, new DynamicAction<TModel>(action));

        }



    }

    internal class FakeImplementationManager<TModel> : IImplementationManager<TModel>
    {
        internal LogicTestEvaluatorFactory ef { get; }
        internal LogicTestActionFactory af { get; }
        internal LogicTestDefintionFactory df { get; }

        IActionFactory IImplementationManager<TModel>.ActionFactory => null;

        IRuleEvaluatorFactory IImplementationManager<TModel>.EvaluatorFactory => null;
        IWorkflowDefinitionFactory IImplementationManager<TModel>.DefinitionFactory => null;


        private readonly WorkDefine.Workflow workFlow;
        private readonly Config configuration;

        public FakeImplementationManager(Case testCase, WorkDefine.Workflow workflow, Configuration.Config configuration)
        {
            this.workFlow = workflow;
            this.configuration = configuration;
            this.ef = new LogicTestEvaluatorFactory(testCase, this.configuration);
            this.af = new LogicTestActionFactory();
            this.df = new LogicTestDefintionFactory(this.configuration);

        }

        public IAction<TModel> GetAction(WorkDefine.ActionDefinition def)
        {


            return this.af.GetAction<TModel>(def);
        }

        public IRuleEvaluator<TModel> GetEvaluator(LogicDefine.Evaluator def)
        {

            return this.ef.GetRuleEvaluator<TModel>(def);
        }
        public WorkDefine.Workflow GetWorkflow(string id)
        {
            return this.df.GetWorkflow(id);
        }

        //IAction<TModel> IImplementationManager<TModel>.GetAction(WorkDefine.ActionDefinition def)
        //{
        //    throw new NotImplementedException();
        //}

        //IRuleEvaluator<TModel> IImplementationManager<TModel>.GetEvaluator(LogicDefine.Evaluator def)
        //{
        //    throw new NotImplementedException();
        //}

        //WorkDefine.Workflow IImplementationManager<TModel>.GetWorkflow(string id)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

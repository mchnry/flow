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

        ProxyActionFactory<TModel> ActionFactory { get; }
        IWorkflowBuilderFactory DefinitionFactory { get; }

        void SetActionFactoryProxy(IActionFactory factory);
        void SetEvaluatorFactoryProxy(IRuleEvaluatorFactory factory);

        ProxyEvaluatorFactory<TModel> EvaluatorFactory { get; }

    }

    internal class ImplementationManager<TModel> : IImplementationManager<TModel>
    {


        public ProxyActionFactory<TModel> ActionFactory { get; set; }
        public ProxyEvaluatorFactory<TModel> EvaluatorFactory { get; set; }
        public IWorkflowBuilderFactory DefinitionFactory { get; set; }

        public Config Configuration { get; }

        internal ImplementationManager()
        {
            this.Configuration = new Config();
            this.ActionFactory = new ProxyActionFactory<TModel>(this.Configuration);
            this.EvaluatorFactory = new ProxyEvaluatorFactory<TModel>(this.Configuration);
            this.DefinitionFactory = new NoWorkflowDefinitionFactory();

        }

        internal ImplementationManager(Configuration.Config configuration) : this()
        {

            this.Configuration = configuration;
        }

        internal ImplementationManager(IWorkflowBuilderFactory definitionFactory, Configuration.Config configuration) : this(configuration)
        {
            this.ActionFactory.Configuration = configuration;
            this.EvaluatorFactory.Configuration = configuration;

            this.DefinitionFactory = definitionFactory;

        }






        public virtual IAction<TModel> GetAction(WorkDefine.ActionDefinition def)
        {
           
            
            return this.ActionFactory.getAction(def);

            
        }

        public virtual WorkDefine.Workflow GetWorkflow(string id)
        {
            Builder<TModel> builder = (Builder<TModel>)this.DefinitionFactory.GetWorkflow<TModel>(id);

            foreach (var action in builder.actions)
            {
                this.ActionFactory.AddAction(action.Key, action.Value);
            }

            foreach (var eval in builder.evaluators)
            {
                this.EvaluatorFactory.AddEvaluator(eval.Key, eval.Value);
            }

  

            WorkDefine.Workflow toReturn = ((IBuilderWorkflow<TModel>)builder).Workflow;

            if (this.Configuration.Ordinal > 0)
            {
                WorkflowManager mgr = new WorkflowManager(toReturn, this.Configuration);
                mgr.RenameWorkflow(string.Format("{0}{1}{2}", toReturn.Id, Configuration.Convention.Delimeter, this.Configuration.Ordinal));
            }

            return toReturn;

        }

        public virtual IRuleEvaluator<TModel> GetEvaluator(LogicDefine.Evaluator def)
        {

            return this.EvaluatorFactory.GetRuleEvaluator(def);
        }

        public void SetActionFactoryProxy(IActionFactory factory)
        {
            this.ActionFactory.proxy = factory;
        }

        public void SetEvaluatorFactoryProxy(IRuleEvaluatorFactory factory)
        {
            this.EvaluatorFactory.proxy = factory;
        }

        //internal virtual void AddEvaluator(string id, Func<IEngineScope<TModel>, LogicEngineTrace, IRuleResult, CancellationToken, Task> evaluator)
        //{
        //    this.evaluators.Add(id, new DynamicEvaluator<TModel>(evaluator));

        //}
        //internal virtual void AddAction(string id, Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action)
        //{

        //    this.actions.Add(id, new DynamicAction<TModel>(action));

        //}



    }

    internal class FakeImplementationManager<TModel> : IImplementationManager<TModel>
    {
        internal LogicTestEvaluatorFactory ef { get; }
        internal LogicTestActionFactory af { get; }
        internal LogicTestDefintionFactory df { get; }

        ProxyActionFactory<TModel> IImplementationManager<TModel>.ActionFactory => null;

        ProxyEvaluatorFactory<TModel> IImplementationManager<TModel>.EvaluatorFactory => null;
        IWorkflowBuilderFactory IImplementationManager<TModel>.DefinitionFactory => null;


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
            return this.df.GetWorkflow<TModel>(id).Workflow;
        }

        public void SetActionFactoryProxy(IActionFactory factory)
        {
            //do nothing
        }

        public void SetEvaluatorFactoryProxy(IRuleEvaluatorFactory factory)
        {
            //do nothing
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

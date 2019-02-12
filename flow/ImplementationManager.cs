using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using System.Linq;
using System.Threading;
using Mchnry.Flow.Diagnostics;
using System.Threading.Tasks;

namespace Mchnry.Flow
{
    internal class ImplementationManager<TModel>
    {

        private readonly WorkDefine.Workflow workFlow;




        //store reference to all factory created actions
        private Dictionary<string, IAction<TModel>> actions = new Dictionary<string, IAction<TModel>>();
        //store references to all factory created evaluators
        private Dictionary<string, IRuleEvaluator<TModel>> evaluators = new Dictionary<string, IRuleEvaluator<TModel>>();

        public IActionFactory ActionFactory { get; set; }
        public IRuleEvaluatorFactory EvaluatorFactory { get; set; }

        internal ImplementationManager()
        {
            this.ActionFactory = new NoActionFactory();
            this.EvaluatorFactory = new NoRuleEvaluatorFactory();

            IRuleEvaluator<TModel> trueEvaluator = new AlwaysTrueEvaluator<TModel>();
            this.evaluators.Add("true", trueEvaluator);
        }

        internal ImplementationManager(IActionFactory actionFactory, IRuleEvaluatorFactory evaluatorFactory, WorkDefine.Workflow workFlow) {
            this.ActionFactory = actionFactory;
            this.EvaluatorFactory = evaluatorFactory;
            this.workFlow = workFlow;


        }

        public virtual IAction<TModel> GetAction(string actionId)
        {
            IAction<TModel> toReturn = default(IAction<TModel>);
            if (!this.actions.ContainsKey(actionId))
            {
                if ("*placeHolder" == actionId)
                {
                    toReturn = new NoAction<TModel>();
                }
                else
                {

                    WorkDefine.ActionDefinition def = this.workFlow.Actions.FirstOrDefault(g => g.Id.Equals(actionId));

                    try
                    {
                        toReturn = this.ActionFactory.GetAction<TModel>(def);
                    }
                    catch (System.Exception ex)
                    {
                        throw new LoadActionException(actionId, ex);
                    }

                    if (default(IAction<TModel>) == toReturn)
                    {
                        throw new LoadActionException(actionId);
                    }

                    this.actions.Add(actionId, toReturn);
                }
            }
            else
            {
                toReturn = this.actions[actionId];
            }
            return toReturn;
        }

        internal virtual IRuleEvaluator<TModel> GetEvaluator(string id)
        {
            IRuleEvaluator<TModel> toReturn = default(IRuleEvaluator<TModel>);

            if (id == "true")
            {
                return new AlwaysTrueEvaluator<TModel>();
            }

            if (!this.evaluators.ContainsKey(id))
            {
                LogicDefine.Evaluator def = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(id));
                try
                {
                    toReturn = this.EvaluatorFactory.GetRuleEvaluator<TModel>(def);
                }
                catch (System.Exception ex)
                {
                    throw new LoadEvaluatorException(id, ex);
                }

                if (default(IRuleEvaluator<TModel>) == toReturn)
                {
                    throw new LoadEvaluatorException(id);
                }

                this.evaluators.Add(def.Id, toReturn);
            }
            else
            {
                toReturn = this.evaluators[id];
            }
            return toReturn;
        }

        internal virtual void AddEvaluator(string id, Func<IEngineScope<TModel>, LogicEngineTrace, CancellationToken, Task<bool>> evaluator)
        {
            this.evaluators.Add(id, new DynamicEvaluator<TModel>(evaluator));
            
        }
        internal virtual void AddAction(string id, Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action)
        {

            this.actions.Add(id, new DynamicAction<TModel>(action));
       
        }



    }
}

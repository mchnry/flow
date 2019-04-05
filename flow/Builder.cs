using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Configuration;
using System.Linq;
using Mchnry.Flow.Work;
using Mchnry.Flow.Logic;

namespace Mchnry.Flow
{

    /// <summary>
    /// Fluent Builder Interface for defining an action to be executed in a flow.
    /// </summary>
    /// <typeparam name="T">Type of model used in flow.</typeparam>
    public interface IActionBuilder<T>
    {
        /// <summary>
        /// Define an action to be executed with context.
        /// </summary>
        /// <param name="action">Implementation of <see cref="Mchnry.Flow.Work.IAction{TModel}"/></param>
        /// <param name="context">Contextual parameter as defined by Action</param>
        void DoWithContext(Mchnry.Flow.Work.IAction<T> action, string context);

        /// <summary>
        /// Define an action to be executed with context.
        /// </summary>
        /// <param name="action">Implementation of <see cref="Mchnry.Flow.Work.IAction{TModel}"/></param>
        void Do(Mchnry.Flow.Work.IAction<T> action);

    }

    /// <summary>
    /// Generic Implementation of <see cref="IActionBuilder{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of model class used in flow</typeparam>
    public class ActionBuilder<T> : IActionBuilder<T>
    {

        internal IAction<T> action { get; set; }
        internal WorkDefine.ActionRef actionRef { get; set; }

        void IActionBuilder<T>.DoWithContext(IAction<T> action, string context)
        {
            ((IActionBuilder<T>)this).Do(action);
            this.actionRef.Context = context;
        }
        void IActionBuilder<T>.Do(IAction<T> action)
        {
            this.action = action;
            this.actionRef = new WorkDefine.ActionRef() { Id = action.Definition.Id };

        }
    }

    /// <summary>
    /// Fluent Interface for defining the true condition for the defined implementation of <see cref="IRuleEvaluator{TModel}"/>
    /// </summary>
    public interface IRuleConditionBuilder
    {
        /// <summary>
        /// The rule will return true if the evaluator returns true.
        /// </summary>
        void IsTrue();
        /// <summary>
        /// The rule will return true if the evaluator returns false.
        /// </summary>
        void IsFalse();
        /// <summary>
        /// The rule will return true if the evaluator returns the value provided.
        /// </summary>
        /// <param name="condition">The true condition of the rule.</param>
        void Is(bool condition);

    }

    /// <summary>
    /// Fluent Interface for defining a rule.
    /// </summary>
    /// <typeparam name="T">Type of model used in flow.</typeparam>
    public interface IRuleBuilder<T>
    {
        /// <summary>
        /// Defines the rule that queries an <see cref="IRuleEvaluator{TModel}"/>
        /// </summary>
        /// <param name="evaluator">Implementation of <see cref="IRule{TModel}"/> to query</param>
        /// <returns>Reference as <see cref="IRuleConditionBuilder"/> to indicate the true condition of evaluator.</returns>
        IRuleConditionBuilder Eval(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator);
        /// <summary>
        /// Defines the rule that queries an <see cref="IRuleEvaluator{TModel}"/>
        /// </summary>
        /// <param name="evaluator">Implementation of <see cref="IRule{TModel}"/> to query</param>
        /// <param name="context">The context to pass to the evaluator.</param>
        /// <returns>Reference as <see cref="IRuleConditionBuilder"/> to indicate the true condition of evaluator.</returns>
        IRuleConditionBuilder EvalWithContext(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator, string context);

    }

    /// <summary>
    /// Implementation of <see cref="IRuleBuilder{T}"/> and <see cref="IRuleConditionBuilder"/> for defining a rule.
    /// </summary>
    /// <typeparam name="T">Type of model used in flow.</typeparam>
    public class RuleBuilder<T> : IRuleBuilder<T>, IRuleConditionBuilder
    {

        internal LogicDefine.Rule rule { get; set; }
        internal Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator { get; set; }

        IRuleConditionBuilder IRuleBuilder<T>.Eval(IRuleEvaluator<T> evaluator)
        {
            this.evaluator = evaluator;
            this.rule = new LogicDefine.Rule() { Id = this.evaluator.Definition.Id, TrueCondition = true };
            return this;
        }
        IRuleConditionBuilder IRuleBuilder<T>.EvalWithContext(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator, string context)
        {
            ((IRuleBuilder<T>)this).Eval(evaluator);
            this.rule.Context = context;
            return this;
        }

        void IRuleConditionBuilder.Is(bool condition)
        {
            this.rule.TrueCondition = condition;
        }

        void IRuleConditionBuilder.IsFalse()
        {
            this.rule.TrueCondition = false;
        }

        void IRuleConditionBuilder.IsTrue()
        {
            this.rule.TrueCondition = true;
        }

    }

    #region Fluent

    /// <summary>
    /// Interface for fluent activity builder.
    /// </summary>
    /// <typeparam name="T">Type of model used in flow</typeparam>
    public interface IFluentActivityBuilder<T>
    {
        /// <summary>
        /// Execute the provided implementation of <see cref="IActionBuilder{T}"/>
        /// </summary>
        /// <param name="Do">Implementation of IActionBuilder</param>
        /// <returns><see cref="IFluentActivityBuilder{T}"/></returns>
        IFluentActivityBuilder<T> Do(Action<IActionBuilder<T>> Do);
        /// <summary>
        /// Chains another workflow to be executed as defined by this workflow.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>The chained workflow must use the same type of model.  Alternatively, can provide
        /// and implementation if <see cref="IAction{TModel}"/> which uses scope to call another workflow of a different model type</item>
        /// </list>
        /// </remarks>
        /// <param name="builder">Implementation of <see cref="IWorkflowBuilder{T}"/></param>
        /// <returns><see cref="IFluentActivityBuilder{T}"/></returns>
        IFluentActivityBuilder<T> Chain(IWorkflowBuilder<T> builder);
        /// <summary>
        /// Placeholder for use while building a flow. This allows for the definition and linting of a flow
        /// before developing the actions.
        /// </summary>
        /// <returns><see cref="IFluentActivityBuilder{T}"/></returns>
        IFluentActivityBuilder<T> DoNothing();

        /// <summary>
        /// Builder for creating a condition reaction in an activity
        /// </summary>
        /// <param name="If">Implementation of <see cref="IFluentExpressionBuilder{T}"/> for defining rule expression</param>
        /// <param name="Then">Implementation of <see cref="IFluentActivityBuilder{T}"/> for defining action</param>
        /// <returns><see cref="IFluentElseActivityBuilder{T}"/> ElseBuilder for quickly defining an else reaction</returns>
        IFluentElseActivityBuilder<T> IfThenDo(Action<IFluentExpressionBuilder<T>> If, Action<IFluentActivityBuilder<T>> Then);

    }

    /// <summary>
    /// Fluent Interface for builing an activity. 
    /// </summary>
    /// <typeparam name="T">Type of model used in flow</typeparam>
    public interface IFluentElseActivityBuilder<T>
    {
        /// <summary>
        /// Execute the provided implementation of <see cref="IActionBuilder{T}"/>
        /// </summary>
        /// <param name="Do">Implementation of IActionBuilder</param>
        /// <returns><see cref="IFluentActivityBuilder{T}"/></returns>
        IFluentActivityBuilder<T> Do(Action<IActionBuilder<T>> Do);
        /// <summary>
        /// Chains another workflow to be executed as defined by this workflow.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>The chained workflow must use the same type of model.  Alternatively, can provide
        /// and implementation if <see cref="IAction{TModel}"/> which uses scope to call another workflow of a different model type</item>
        /// </list>
        /// </remarks>
        /// <param name="builder">Implementation of <see cref="IWorkflowBuilder{T}"/></param>
        /// <returns><see cref="IFluentActivityBuilder{T}"/></returns>
        IFluentActivityBuilder<T> Chain(IWorkflowBuilder<T> builder);
        /// <summary>
        /// Builder for creating a condition reaction in an activity
        /// </summary>
        /// <param name="If">Implementation of <see cref="IFluentExpressionBuilder{T}"/> for defining rule expression</param>
        /// <param name="Then">Implementation of <see cref="IFluentActivityBuilder{T}"/> for defining action</param>
        /// <returns><see cref="IFluentElseActivityBuilder{T}"/> ElseBuilder for quickly defining an else reaction</returns>
        IFluentElseActivityBuilder<T> IfThenDo(Action<IFluentExpressionBuilder<T>> If, Action<IFluentActivityBuilder<T>> Then);
        /// <summary>
        /// Defines the activity to execute if the prior activitie's condition evaluaed to false
        /// </summary>
        /// <param name="Then">Implementation of <see cref="IFluentActivityBuilder{T}"/> to run</param>
        /// <returns></returns>
        IFluentActivityBuilder<T> Else(Action<IFluentActivityBuilder<T>> Then);

    }


    /// <summary>
    /// Fluent Interface for defining a rule expression.
    /// </summary>
    /// <typeparam name="T">Type of model used in flow.</typeparam>
    public interface IFluentExpressionBuilder<T>
    {
        /// <summary>
        /// Defines a rule
        /// </summary>
        /// <param name="rule">Implementation of <see cref="IRule{TModel}"/></param>
        void Rule(Action<IRuleBuilder<T>> rule);
        //void RefIsTrue(ExpressionRef xref);

        /// <summary>
        /// Defines a rule where the result of the Expression defined by implementation of <see cref="IFluentExpressionBuilder{T}"/> must be true.
        /// </summary>
        /// <param name="If">Implementation of <see cref="IFluentExpressionBuilder{T}"/></param>
        void ExpIsTrue(Action<IFluentExpressionBuilder<T>> If);

        /// <summary>
        /// Defines a rule where the result of the Expression defined by implementation of <see cref="IFluentExpressionBuilder{T}"/> must be false.
        /// </summary>
        /// <param name="If">Implementation of <see cref="IFluentExpressionBuilder{T}"/></param>
        void ExpIsFalse(Action<IFluentExpressionBuilder<T>> If);

        /// <summary>
        /// Defines an expression where the results of the first and second expressions must both be true.
        /// </summary>
        /// <param name="first">Implementation of <see cref="IFluentExpressionBuilder{T}"/></param>
        /// <param name="second">Implementation of <see cref="IFluentExpressionBuilder{T}"/></param>
        void And(Action<IFluentExpressionBuilder<T>> first, Action<IFluentExpressionBuilder<T>> second);
        /// <summary>
        /// Defines an expression where at least one of the results of the first and second expressions must be true.
        /// </summary>
        /// <param name="first">Implementation of <see cref="IFluentExpressionBuilder{T}"/></param>
        /// <param name="second">Implementation of <see cref="IFluentExpressionBuilder{T}"/></param>
        void Or(Action<IFluentExpressionBuilder<T>> first, Action<IFluentExpressionBuilder<T>> second);

    }



    #endregion

    #region Not Fluent

    public interface IActivityBuilder
    {
        IActivityBuilder Do(WorkDefine.ActionRef action);
        IActivityBuilder DoNothing();

        IElseActivityBuilder IfThenDo(Action<IExpressionBuilder> If, Action<IActivityBuilder> Then);

    }
    public interface IElseActivityBuilder
    {
        IActivityBuilder Do(WorkDefine.ActionRef action);
        IElseActivityBuilder IfThenDo(Action<IExpressionBuilder> If, Action<IActivityBuilder> Then);
        IActivityBuilder Else(Action<IActivityBuilder> Then);

    }

    public interface IExpressionBuilder
    {
        void True(LogicDefine.Rule evaluatorId);
        //void True(ExpressionRef xref);

        void And(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);
        void Or(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);

    }

    #endregion


    /// <summary>
    /// Interface for building a flow.
    /// </summary>
    /// <typeparam name="T">Type of model used in flow.</typeparam>
    public interface IBuilder<T>
    {
        /// <summary>
        /// Fluent Builder
        /// </summary>
        /// <param name="Activity"></param>
        /// <returns></returns>
        IBuilderWorkflow<T> BuildFluent(Action<IFluentActivityBuilder<T>> Activity);
        IBuilderWorkflow<T> Build(Action<IActivityBuilder> Activity);
    }

    public interface IBuilderWorkflow<T>
    {
        WorkDefine.Workflow Workflow { get; }
    }



    public class Builder<T> : 
        IBuilder<T>, 
        IBuilderWorkflow<T>, 
        IFluentExpressionBuilder<T>, 
        IFluentActivityBuilder<T>, 
        IFluentElseActivityBuilder<T>, 
        IExpressionBuilder, 
        IActivityBuilder, 
        IElseActivityBuilder
    {

        internal Dictionary<string, IRuleEvaluator<T>> evaluators = new Dictionary<string, IRuleEvaluator<T>>();
        internal Dictionary<string, IAction<T>> actions = new Dictionary<string, IAction<T>>();

          

        internal WorkflowManager workflowManager;

        internal Stack<WorkDefine.Activity> activityStack = new Stack<WorkDefine.Activity>();
        internal Stack<LogicDefine.IExpression> epxressionStack = new Stack<LogicDefine.IExpression>();
        internal Config config = new Config();
        internal WorkDefine.ActionRef created = null;
        internal Dictionary<string, int> subActivities = new Dictionary<string, int>();
        internal string WorkflowId;
        internal LogicDefine.IExpression LastEquation;

        public static IBuilder<T> CreateBuilder(string workflowId)
        {
            return new Builder<T>(workflowId);
        }

        public static IBuilder<T> CreateBuilder(string workflowId, Action<Configuration.Config> configure)
        {
            return new Builder<T>(workflowId, configure);
        }

        internal Builder(string workflowId): this(workflowId, null)
        {
            
        }
        public Builder (string workflowId, Action<Configuration.Config> configure)
        {
            configure?.Invoke(this.config);

            
            WorkDefine.Workflow workflow = new WorkDefine.Workflow(workflowId)
            {
                Actions = new List<WorkDefine.ActionDefinition>(),
                Activities = new List<WorkDefine.Activity>(),
                Equations = new List<LogicDefine.Equation>(),
                Evaluators = new List<LogicDefine.Evaluator>()
            };
            this.workflowManager = new WorkflowManager(workflow, this.config);
        }

        private void  Do(Action<IActionBuilder<T>> builder)
        {
            ActionBuilder<T> builderRef = new ActionBuilder<T>();
            builder.Invoke(builderRef);

            WorkDefine.ActionRef ToDo = builderRef.actionRef;
            
            ToDo.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Action, ToDo.Id, this.config.Convention);
            this.workflowManager.AddAction(new WorkDefine.ActionDefinition() { Id = ToDo.Id, Description = builderRef.action.Definition.Description });

            if (!this.actions.ContainsKey(ToDo.Id))
            {
                this.actions.Add(ToDo.Id, builderRef.action);
            }

            this.created = ToDo;
        }

        private void Do(WorkDefine.ActionRef ToDo)
        {
            ToDo.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Action, ToDo.Id, this.config.Convention);
            this.workflowManager.AddAction(new WorkDefine.ActionDefinition() { Id = ToDo.Id, Description = "Builder" });
            this.created = ToDo;
        }

        IActivityBuilder IActivityBuilder.DoNothing()
        {
            return ((IActivityBuilder)this).Do("noaction");
        }
        IFluentActivityBuilder<T> IFluentActivityBuilder<T>.DoNothing()
        {

            return ((IFluentActivityBuilder<T>)this).Do(z => z.Do(new NoAction<T>()));


        }


        private void Build()
        {
            string workflowId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, this.workflowManager.WorkFlow.Id, this.config.Convention);
            workflowId = workflowId + this.config.Convention.Delimeter + "Main";

            WorkDefine.Activity parent = new WorkDefine.Activity()
            {
                Id = workflowId,
                Reactions = new List<WorkDefine.Reaction>() { }
            };
            this.activityStack.Push(parent);
            this.workflowManager.AddActivity(parent);
            this.subActivities.Add(parent.Id, 0);
        }
        IBuilderWorkflow<T> IBuilder<T>.Build(Action<IActivityBuilder> First)
        {
            this.Build();

            First(this);

            return this;
        }
        IBuilderWorkflow<T> IBuilder<T>.BuildFluent(Action<IFluentActivityBuilder<T>> First)
        {

            this.Build();

            First(this);

            return this;

        }

        WorkDefine.Workflow IBuilderWorkflow<T>.Workflow {
            get {
                return this.workflowManager.WorkFlow;
            }
        }

        IFluentActivityBuilder<T> IFluentElseActivityBuilder<T>.Do(Action<IActionBuilder<T>> builder) { return ((IFluentActivityBuilder<T>)this).Do(builder); }
        IFluentActivityBuilder<T> IFluentActivityBuilder<T>.Do(Action<IActionBuilder<T>> builder)
        {

            WorkDefine.Activity parent = default(WorkDefine.Activity);
            //get the parent, add this as a reaction

            parent = this.activityStack.Peek();


            this.Do(builder);
            parent.Reactions.Add(new WorkDefine.Reaction() { Work = this.created.ToString() });

            return this;

        }

        IFluentActivityBuilder<T> IFluentElseActivityBuilder<T>.Chain(IWorkflowBuilder<T> builder) { return ((IFluentActivityBuilder<T>)this).Chain(builder); }
        IFluentActivityBuilder<T> IFluentActivityBuilder<T>.Chain(IWorkflowBuilder<T> builder)
        {

            WorkDefine.Activity parent = default(WorkDefine.Activity);
            //get the parent, add this as a reaction

            parent = this.activityStack.Peek();

            string workflowId = builder.GetBuilder().Workflow.Id;

            string actionId = $"chain{workflowId}";
            this.Do(a => a.Do(new ChainFlowAction<T>(actionId, workflowId)));

            parent.Reactions.Add(new WorkDefine.Reaction() { Work = this.created.ToString() });

            return this;

        }

        IActivityBuilder IElseActivityBuilder.Do(WorkDefine.ActionRef action) { return ((IActivityBuilder)this).Do(action); }
        IActivityBuilder IActivityBuilder.Do(WorkDefine.ActionRef action)
        {

            WorkDefine.Activity parent = default(WorkDefine.Activity);
            //get the parent, add this as a reaction

            parent = this.activityStack.Peek();


            this.Do(action);
            parent.Reactions.Add(new WorkDefine.Reaction() { Work = this.created.ToString() });

            return this;

        }



        IFluentElseActivityBuilder<T> IFluentElseActivityBuilder<T>.IfThenDo(Action<IFluentExpressionBuilder<T>> If, Action<IFluentActivityBuilder<T>> Then) { return ((IFluentActivityBuilder<T>)this).IfThenDo(If, Then); }
        IFluentElseActivityBuilder<T> IFluentActivityBuilder<T>.IfThenDo(Action<IFluentExpressionBuilder<T>> If, Action<IFluentActivityBuilder<T>> Then)
        {
            WorkDefine.Activity parent = this.activityStack.Peek();

            int subCount = this.subActivities[parent.Id];
            subCount++;
            this.subActivities[parent.Id] = subCount;

            string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = activityId,
                Reactions = new List<WorkDefine.Reaction>() { }
            };

            

            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);
            this.workflowManager.AddActivity(toBuild);

            If(this);
            Then(this);

            LastEquation = this.epxressionStack.Pop();
            
            parent.Reactions.Add(new WorkDefine.Reaction() { Logic = LastEquation.ShortHand, Work = toBuild.Id });

            this.activityStack.Pop();

            return this;
        }

        IElseActivityBuilder IElseActivityBuilder.IfThenDo(Action<IExpressionBuilder> If, Action<IActivityBuilder> Then) { return ((IActivityBuilder)this).IfThenDo(If, Then); }
        IElseActivityBuilder IActivityBuilder.IfThenDo(Action<IExpressionBuilder> If, Action<IActivityBuilder> Then)
        {
            WorkDefine.Activity parent = this.activityStack.Peek();

            int subCount = this.subActivities[parent.Id];
            subCount++;
            this.subActivities[parent.Id] = subCount;

            string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = activityId,
                Reactions = new List<WorkDefine.Reaction>() { }
            };



            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);
            this.workflowManager.AddActivity(toBuild);

            If(this);
            Then(this);

            LastEquation = this.epxressionStack.Pop();

            parent.Reactions.Add(new WorkDefine.Reaction() { Logic = LastEquation.ShortHand, Work = toBuild.Id });

            this.activityStack.Pop();

            return this;
        }

        IFluentActivityBuilder<T> IFluentElseActivityBuilder<T>.Else(Action<IFluentActivityBuilder<T>> Then)
        {
            WorkDefine.Activity parent = this.activityStack.Peek();

            int subCount = this.subActivities[parent.Id];
            subCount++;
            this.subActivities[parent.Id] = subCount;

            string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = activityId,
                Reactions = new List<WorkDefine.Reaction>() { }
            };



            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);
            this.workflowManager.AddActivity(toBuild);


            LogicDefine.Rule equationAsRule = LastEquation.ShortHand;
            //negate
            equationAsRule.TrueCondition = !equationAsRule.TrueCondition;

            string equationId = equationAsRule.ShortHand;           
            Then(this);



            parent.Reactions.Add(new WorkDefine.Reaction() { Logic = equationId, Work = toBuild.Id });

            this.activityStack.Pop();

            return this;
        }

        IActivityBuilder IElseActivityBuilder.Else(Action<IActivityBuilder> Then)
        {
            WorkDefine.Activity parent = this.activityStack.Peek();

            int subCount = this.subActivities[parent.Id];
            subCount++;
            this.subActivities[parent.Id] = subCount;

            string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = activityId,
                Reactions = new List<WorkDefine.Reaction>() { }
            };



            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);
            this.workflowManager.AddActivity(toBuild);

            LogicDefine.Rule equationAsRule = LastEquation.ShortHand;
            //negate
            equationAsRule.TrueCondition = !equationAsRule.TrueCondition;

            string equationId = equationAsRule.ShortHand;

            Then(this);



            parent.Reactions.Add(new WorkDefine.Reaction() { Logic = equationId, Work = toBuild.Id });

            this.activityStack.Pop();

            return this;
        }

        void IFluentExpressionBuilder<T>.ExpIsTrue(Action<IFluentExpressionBuilder<T>> If)
        {
            string equationId = string.Empty;


            //we are in a sub equation
            if (this.epxressionStack.Count > 0)
            {
                string lastEquationId = this.epxressionStack.Peek().Id;
                string suffix = (this.epxressionStack.Count % 2 == 0) ? "2" : "1";
                equationId = lastEquationId + this.config.Convention.Delimeter + suffix;

            }
            else //we are at the root
            {
                string lastActivityId = this.activityStack.Peek().Id;
                equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Activity, NamePrefixOptions.Equation, lastActivityId, this.config.Convention);

            }

            LogicDefine.Equation toAdd = new LogicDefine.Equation()
            {
                Condition = Logic.Operand.And,
                Id = equationId
            };

            this.epxressionStack.Push(toAdd);
            this.workflowManager.AddEquation(toAdd);


            string firstId, secondId = null;
            If(this);
            firstId = this.epxressionStack.Pop().ShortHand;

            secondId = ConventionHelper.TrueEvaluator(this.config.Convention);


            toAdd.First = firstId;
            toAdd.Second = secondId;

            
        }
        void IFluentExpressionBuilder<T>.ExpIsFalse(Action<IFluentExpressionBuilder<T>> If)
        {
            string equationId = string.Empty;


            //we are in a sub equation
            if (this.epxressionStack.Count > 0)
            {
                string lastEquationId = this.epxressionStack.Peek().Id;
                string suffix = (this.epxressionStack.Count % 2 == 0) ? "2" : "1";
                equationId = lastEquationId + this.config.Convention.Delimeter + suffix;

            }
            else //we are at the root
            {
                string lastActivityId = this.activityStack.Peek().Id;
                equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Activity, NamePrefixOptions.Equation, lastActivityId, this.config.Convention);

            }

            LogicDefine.Equation toAdd = new LogicDefine.Equation()
            {
                Condition = Logic.Operand.And,
                Id = equationId
            };

            this.epxressionStack.Push(toAdd);
            this.workflowManager.AddEquation(toAdd);


            string firstId, secondId = null;
            If(this);

            LogicDefine.Rule firstRule = this.epxressionStack.Pop().ShortHand;
            firstRule.TrueCondition = !firstRule.TrueCondition;

            firstId = firstRule.ShortHand;

            secondId = ConventionHelper.TrueEvaluator(this.config.Convention);


            toAdd.First = firstId;
            toAdd.Second = secondId;
        }
        void IFluentExpressionBuilder<T>.And(Action<IFluentExpressionBuilder<T>> first, Action<IFluentExpressionBuilder<T>> second)
        {

            string equationId = string.Empty;


            //we are in a sub equation
            if (this.epxressionStack.Count > 0)
            {
                string lastEquationId = this.epxressionStack.Peek().Id;
                string suffix = (this.epxressionStack.Count % 2 == 0) ? "2" : "1";
                equationId = lastEquationId + this.config.Convention.Delimeter + suffix;

            } else //we are at the root
            {
                string lastActivityId = this.activityStack.Peek().Id;
                equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Activity, NamePrefixOptions.Equation, lastActivityId, this.config.Convention);
            
            }

            LogicDefine.Equation toAdd = new LogicDefine.Equation()
            {
                Condition = Logic.Operand.And,
                Id = equationId
            };

            this.epxressionStack.Push(toAdd);
            this.workflowManager.AddEquation(toAdd);
            

            string firstId, secondId = null;
            first(this);
            firstId = this.epxressionStack.Pop().ShortHand;
            second(this);
            secondId = this.epxressionStack.Pop().ShortHand;
            

            toAdd.First = firstId;
            toAdd.Second = secondId;

            //return new ExpressionRef(toAdd.ShortHand);
    
        }

        void IExpressionBuilder.And(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second)
        {

            string equationId = string.Empty;


            //we are in a sub equation
            if (this.epxressionStack.Count > 0)
            {
                string lastEquationId = this.epxressionStack.Peek().Id;
                string suffix = (this.epxressionStack.Count % 2 == 0) ? "2" : "1";
                equationId = lastEquationId + this.config.Convention.Delimeter + suffix;

            }
            else //we are at the root
            {
                string lastActivityId = this.activityStack.Peek().Id;
                equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Activity, NamePrefixOptions.Equation, lastActivityId, this.config.Convention);

            }

            LogicDefine.Equation toAdd = new LogicDefine.Equation()
            {
                Condition = Logic.Operand.And,
                Id = equationId
            };

            this.epxressionStack.Push(toAdd);
            this.workflowManager.AddEquation(toAdd);


            string firstId, secondId = null;
            first(this);
            firstId = this.epxressionStack.Pop().ShortHand;
            second(this);
            secondId = this.epxressionStack.Pop().ShortHand;


            toAdd.First = firstId;
            toAdd.Second = secondId;

            // return new ExpressionRef(toAdd.ShortHand);

        }

        void IFluentExpressionBuilder<T>.Or(Action<IFluentExpressionBuilder<T>> first, Action<IFluentExpressionBuilder<T>> second)
        {
            string equationId = string.Empty;
            //we are in a sub equation
            if (this.epxressionStack.Count > 0)
            {
                string lastEquationId = this.epxressionStack.Peek().Id;
                string suffix = (this.epxressionStack.Count % 2 == 0) ? "2" : "1";
                equationId = lastEquationId + this.config.Convention.Delimeter + suffix;

            }
            else //we are at the root
            {
                string lastActivityId = this.activityStack.Peek().Id;
                equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Activity, NamePrefixOptions.Equation, lastActivityId, this.config.Convention);

            }

            LogicDefine.Equation toAdd = new LogicDefine.Equation()
            {
                Condition = Logic.Operand.Or,
                Id = equationId
            };

            this.epxressionStack.Push(toAdd);
            this.workflowManager.AddEquation(toAdd);

            string firstId, secondId = null;
            first(this);
            firstId = this.epxressionStack.Pop().ShortHand;
            
            second(this);
            secondId = this.epxressionStack.Pop().ShortHand;

            toAdd.First = firstId;
            toAdd.Second = secondId;

            //return new ExpressionRef(toAdd.ShortHand);
        }

        void IExpressionBuilder.Or(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second)
        {
            string equationId = string.Empty;
            //we are in a sub equation
            if (this.epxressionStack.Count > 0)
            {
                string lastEquationId = this.epxressionStack.Peek().Id;
                string suffix = (this.epxressionStack.Count % 2 == 0) ? "2" : "1";
                equationId = lastEquationId + this.config.Convention.Delimeter + suffix;

            }
            else //we are at the root
            {
                string lastActivityId = this.activityStack.Peek().Id;
                equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Activity, NamePrefixOptions.Equation, lastActivityId, this.config.Convention);

            }

            LogicDefine.Equation toAdd = new LogicDefine.Equation()
            {
                Condition = Logic.Operand.Or,
                Id = equationId
            };

            this.epxressionStack.Push(toAdd);
            this.workflowManager.AddEquation(toAdd);

            string firstId, secondId = null;
            first(this);
            firstId = this.epxressionStack.Pop().ShortHand;

            second(this);
            secondId = this.epxressionStack.Pop().ShortHand;

            toAdd.First = firstId;
            toAdd.Second = secondId;

            //return new ExpressionRef(toAdd.ShortHand);
        }

        void IFluentExpressionBuilder<T>.Rule(Action<IRuleBuilder<T>> action)
        {

            RuleBuilder<T> builderRef = new RuleBuilder<T>();
            action.Invoke(builderRef);

            LogicDefine.Rule evaluatorId = builderRef.rule;

           

            evaluatorId.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Evaluator, evaluatorId.Id, this.config.Convention);
            this.workflowManager.AddEvaluator(new LogicDefine.Evaluator() { Id = evaluatorId.Id, Description = builderRef.evaluator.Definition.Description });
            bool isRoot = this.epxressionStack.Count == 0;

            if (!this.evaluators.ContainsKey(evaluatorId.Id))
            {
                this.evaluators.Add(evaluatorId.Id, builderRef.evaluator);
            }

            if (isRoot)
            {
                string equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Equation, evaluatorId.Id, this.config.Convention);
                if (!evaluatorId.TrueCondition)
                {
                    equationId = ConventionHelper.NegateEquationName(equationId, this.config.Convention);
                }
                LogicDefine.Equation toAdd = new LogicDefine.Equation()
                {
                    Condition = Logic.Operand.And,
                    First = evaluatorId,
                    Id = equationId,
                    Second = ConventionHelper.TrueEvaluator(this.config.Convention)
                };
                this.epxressionStack.Push(toAdd);

                this.workflowManager.AddEquation(toAdd);
            } else
            {
                this.epxressionStack.Push(evaluatorId);
            }
            //if root... then create euqations
            //otherwise, just use as evaluator
            
            


        }

        //void IFluentExpressionBuilder<T>.RefIsTrue(ExpressionRef xref) { ((IExpressionBuilder)this).True(xref); }
        //void IExpressionBuilder.True(ExpressionRef xref)
        //{
        //    //all the work has been done, we just need to eval negate and push to stack
        //    LogicDefine.Rule ev = xref.Id;

        //    if (xref.negate)
        //    {
        //        ev.TrueCondition = !ev.TrueCondition;
        //    }
        //    this.epxressionStack.Push(ev);

        //}

        void IExpressionBuilder.True(LogicDefine.Rule evaluatorId)
        {

            evaluatorId.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Evaluator, evaluatorId.Id, this.config.Convention);
            this.workflowManager.AddEvaluator(new LogicDefine.Evaluator() { Id = evaluatorId.Id, Description = "Builder" });
            bool isRoot = this.epxressionStack.Count == 0;

            //if we're not already in an equation (i.e. the conditional is just one evaluator) create a root equation where the second
            //operand is just true
            if (isRoot)
            {
                string equationId = ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Equation, evaluatorId.Id, this.config.Convention);
                if (!evaluatorId.TrueCondition)
                {
                    equationId = ConventionHelper.NegateEquationName(equationId, this.config.Convention);
                }
                LogicDefine.Equation toAdd = new LogicDefine.Equation()
                {
                    Condition = Logic.Operand.And,
                    First = evaluatorId,
                    Id = equationId,
                    Second = ConventionHelper.TrueEvaluator(this.config.Convention)
                };
                this.epxressionStack.Push(toAdd);

                this.workflowManager.AddEquation(toAdd);
            }
            else
            {
                this.epxressionStack.Push(evaluatorId);
            }
            //if root... then create euqations
            //otherwise, just use as evaluator




        }

    }
}

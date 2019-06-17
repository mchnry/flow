using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Configuration;
using System.Linq;
using Mchnry.Flow.Work;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

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
        void DoWithContext(Mchnry.Flow.Work.IAction<T> action, string input);

        /// <summary>
        /// Define an action to be executed with context.
        /// </summary>
        /// <param name="action">Implementation of <see cref="Mchnry.Flow.Work.IAction{TModel}"/></param>
        void Do(Mchnry.Flow.Work.IAction<T> action);

        /// <summary>
        /// Do an inline action
        /// </summary>
        /// <param name="actionName">Name of action</param>
        /// <param name="action">Func to do</param>
        void DoInLine(string actionName, string description, string input, Func<IEngineScope<T>, IEngineTrace, CancellationToken, Task<bool>> action);


    }

    /// <summary>
    /// Generic Implementation of <see cref="IActionBuilder{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of model class used in flow</typeparam>
    internal class ActionBuilder<T> : IActionBuilder<T>
    {
        private readonly Builder<T> builderRef;

        internal ActionBuilder(Builder<T> builderRef) {
            this.builderRef = builderRef;
        }

        internal IAction<T> action { get; set; }
        internal WorkDefine.ActionRef actionRef { get; set; }
   

        void IActionBuilder<T>.DoWithContext(IAction<T> action, string input )
        {
            ContextBuilder builder = new ContextBuilder();

            if (string.IsNullOrEmpty(input)) throw new ArgumentException("Caller failed to provide context");

            ((IActionBuilder<T>)this).Do(action);
            
            builderRef.workflowManager.AddContextDefinition(builder.builder.definition);
            this.actionRef.Input = input;
        }

        void IActionBuilder<T>.Do(IAction<T> action)
        {
            this.action = action;

            string typeName = action.GetType().Name;

            this.actionRef = new WorkDefine.ActionRef() { Id = typeName };

        }

        void IActionBuilder<T>.DoInLine(string actionName, string description, string input, Func<IEngineScope<T>, IEngineTrace, CancellationToken, Task<bool>> actionToDo)
        {
            ContextBuilder builder = new ContextBuilder();
            Context ctx = default;

            //if (context != null)
            //{
            //    context.Invoke(builder);
            //    builderRef.workflowManager.AddContextDefinition(builder.builder.definition);
            //    ctx = builder.context;
            //}
            this.action = new DynamicAction<T>(new WorkDefine.ActionDefinition() { Id = actionName, Description = description??"Dynamic" }, actionToDo);
            this.actionRef = new WorkDefine.ActionRef() { Id = actionName, Input = input };
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
        IRuleConditionBuilder EvalWithContext(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator, Action<ContextBuilder> context);

        /// <summary>
        /// Evaluate inline evaluator function
        /// </summary>
        /// <param name="evaluatorName">Name of evaluator</param>
        /// <param name="evaluator">Func to evaluate</param>
        /// <returns></returns>
        IRuleConditionBuilder EvalInLine(string evaluatorName, string description, Action<ContextBuilder> context, Func<IEngineScope<T>, IEngineTrace, IRuleResult, CancellationToken, Task> evaluator);

    }

    /// <summary>
    /// Implementation of <see cref="IRuleBuilder{T}"/> and <see cref="IRuleConditionBuilder"/> for defining a rule.
    /// </summary>
    /// <typeparam name="T">Type of model used in flow.</typeparam>
    internal class RuleBuilder<T> : IRuleBuilder<T>, IRuleConditionBuilder
    {
        private readonly Builder<T> builderRef;

        internal RuleBuilder(Builder<T> builderRef)
        {
            this.builderRef = builderRef;
        }
        internal LogicDefine.Rule rule { get; set; }
        internal Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator { get; set; }

        IRuleConditionBuilder IRuleBuilder<T>.Eval(IRuleEvaluator<T> evaluator)
        {
            this.evaluator = evaluator;
            string name = evaluator.GetType().Name;
            this.rule = new LogicDefine.Rule() { Id = name, TrueCondition = true };
            return this;
        }

        IRuleConditionBuilder IRuleBuilder<T>.EvalInLine(string evaluatorName, string description, Action<ContextBuilder> context, Func<IEngineScope<T>, IEngineTrace, IRuleResult, CancellationToken, Task> evaluatorToEval)
        {
            ContextBuilder builder = new ContextBuilder();
            Context ctx = default;

            if (context != null)
            {
                context.Invoke(builder);
                builderRef.workflowManager.AddContextDefinition(builder.builder.definition);
                ctx = builder.context;
            }

            this.evaluator = new DynamicEvaluator<T>(new LogicDefine.Evaluator() { Id = evaluatorName, Description = description??"Dynamic" }, evaluatorToEval);
            this.rule = new LogicDefine.Rule() { Id = evaluatorName, TrueCondition = true, Context = ctx };
            return this;
        }

        IRuleConditionBuilder IRuleBuilder<T>.EvalWithContext(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator, Action<ContextBuilder> context)
        {

            ((IRuleBuilder<T>)this).Eval(evaluator);
            
            ContextBuilder builder = new ContextBuilder();

            if (context == null) throw new ArgumentException("Caller failed to provide context");

            
            context.Invoke(builder);
            builderRef.workflowManager.AddContextDefinition(builder.builder.definition);
            this.rule.Context = builder.context;

            

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
        //IBuilderWorkflow<T> Build(Action<IActivityBuilder> Activity);

        ReadOnlyCollection<IAction<T>> Actions { get; }
        ReadOnlyCollection<IRuleEvaluator<T>> Evaluators { get; }
        ReadOnlyCollection<IWorkflowBuilder<T>> Chained { get; }
    }

    public interface IBuilderWorkflow<T>
    {
        WorkDefine.Workflow Workflow { get; }
    }
     
    public class ContextDefinitionBuilder
    {

        internal ContextDefinition definition { get; private set; }

        internal ContextDefinitionBuilder() { }

        public void OneOf(string name, string literal, IEnumerable<ContextItem> items, bool exclusive)
        {
            this.definition = new ContextDefinition(name, literal, items, ValidateOptions.OneOf, exclusive);
            
            
        }

        public void AnyOf(string name, string literal, IEnumerable<ContextItem> items, bool exclusive)
        {
            this.definition = new ContextDefinition(name, literal, items, ValidateOptions.AnyOf, exclusive);
            
            
        }
        public void OneOf(string name, string literal, Type enumType, bool exclusive)
        {
            this.definition = new ContextDefinition(name, literal, this.FromEnum(enumType), ValidateOptions.OneOf, exclusive);


        }

        public void AnyOf(string name, string literal, Type enumType, bool exclusive)
        {



            this.definition = new ContextDefinition(name, literal, this.FromEnum(enumType), ValidateOptions.AnyOf, exclusive);


        }

        private List<ContextItem> FromEnum(Type enumType)
        {

            Type baseType = enumType.GetEnumUnderlyingType();

            List<ContextItem> toReturn = new List<ContextItem>();
            var enumValues = Enum.GetValues(enumType);
            foreach(object o in enumValues)
            {
                object key = Convert.ChangeType(o, baseType);
                string value = Enum.GetName(enumType, o);
                toReturn.Add(new ContextItem() { Key = key.ToString(), Literal = value });
            }
        
            return toReturn;
        }

    }

    

    
    public class ContextBuilder
    {

        internal ContextDefinitionBuilder builder { get; private set; }
        internal Context context { get; private set; }
        //this.workflowManager.AddContextDefinition(toAdd);

        internal ContextBuilder()
        {
            this.builder = new ContextDefinitionBuilder();
        }
 
        public void MatchAny(Action<ContextDefinitionBuilder> builder, IEnumerable<string> onKeys)
        {
            builder.Invoke(this.builder);

            this.context = new Context(onKeys, this.builder.definition.Name);
        }

        public void Match(Action<ContextDefinitionBuilder> builder, string onKey)
        {
            builder.Invoke(this.builder);
            this.context = new Context(new string[] { onKey }, this.builder.definition.Name);
        }


    }




    public class Builder<T> : 
        IBuilder<T>, 
        IBuilderWorkflow<T>, 
        IFluentExpressionBuilder<T>, 
        IFluentActivityBuilder<T>, 
        IFluentElseActivityBuilder<T>
    {

        internal Dictionary<string, IRuleEvaluator<T>> evaluators = new Dictionary<string, IRuleEvaluator<T>>();
        internal Dictionary<string, IAction<T>> actions = new Dictionary<string, IAction<T>>();
        internal Dictionary<string, IWorkflowBuilder<T>> chained = new Dictionary<string, IWorkflowBuilder<T>>();
          

        internal WorkflowManager workflowManager;

        internal Stack<WorkDefine.Activity> activityStack = new Stack<WorkDefine.Activity>();
        internal Stack<LogicDefine.IExpression> epxressionStack = new Stack<LogicDefine.IExpression>();
        internal Config config = new Config();
        internal WorkDefine.ActionRef created = null;
        internal Dictionary<string, int> subActivities = new Dictionary<string, int>();
        internal string WorkflowId;
        internal LogicDefine.IExpression LastEquation;

        ReadOnlyCollection<IAction<T>> IBuilder<T>.Actions => (from a in actions select a.Value).ToList().AsReadOnly();
        ReadOnlyCollection<IRuleEvaluator<T>> IBuilder<T>.Evaluators => (from a in evaluators select a.Value).ToList().AsReadOnly();
        ReadOnlyCollection<IWorkflowBuilder<T>> IBuilder<T>.Chained => (from a in chained select a.Value).ToList().AsReadOnly();
        

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
                Evaluators = new List<LogicDefine.Evaluator>(),
                ContextDefinitions = new List<ContextDefinition>()
            };
            this.workflowManager = new WorkflowManager(workflow, this.config);
        }

        private void  Do(Action<IActionBuilder<T>> builder)
        {
            ActionBuilder<T> builderRef = new ActionBuilder<T>(this);
            builder.Invoke(builderRef);

            WorkDefine.ActionRef ToDo = builderRef.actionRef;
            
            ToDo.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Action, ToDo.Id, this.config.Convention);

            string actionName = builderRef.action.GetType().Name;
            string description = ConventionHelper.ParseMethodName(actionName, this.config.Convention.ParseMethodNamesAs).Literal;

            var descAttr = builderRef.action.GetType().GetCustomAttributes(typeof(ArticulateOptionsAttribute), true)
                 .OfType<ArticulateOptionsAttribute>()
                 .FirstOrDefault();
            if (descAttr != null)
            {
                description = descAttr.Description;
            }


            this.workflowManager.AddAction(new WorkDefine.ActionDefinition() { Id = ToDo.Id, Description = description });

            if (!this.actions.ContainsKey(ToDo.Id))
            {
            
                this.actions.Add(ToDo.Id, builderRef.action);
            } else
            {
                //if attmpeting to add another implementation with the same id, throw an exception
                //we can't handle this
                if ((this.actions[ToDo.Id].GetType()) != builderRef.action.GetType())
                {
                    throw new BuilderException(ToDo.Id);
                }
            }

            this.created = ToDo;
        }

        private void Do(WorkDefine.ActionRef ToDo)
        {
            ToDo.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Action, ToDo.Id, this.config.Convention);
            this.workflowManager.AddAction(new WorkDefine.ActionDefinition() { Id = ToDo.Id, Description = "Builder" });
            this.created = ToDo;
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

            if (this.chained.ContainsKey(WorkflowId))
            {
                workflowId = workflowId + (this.chained.Count() + 1).ToString();
            }

            this.chained.Add(workflowId, builder);

            string actionId = $"chain{workflowId}";
            this.Do(a => a.Do(new ChainFlowAction<T>(actionId, builder)));

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


        void IFluentExpressionBuilder<T>.Rule(Action<IRuleBuilder<T>> action)
        {

            RuleBuilder<T> builderRef = new RuleBuilder<T>(this);
            action.Invoke(builderRef);

            LogicDefine.Rule evaluatorId = builderRef.rule;

           

            evaluatorId.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Evaluator, evaluatorId.Id, this.config.Convention);

            string actionName = builderRef.evaluator.GetType().Name;
            string description = ConventionHelper.ParseMethodName(actionName, this.config.Convention.ParseMethodNamesAs).Literal;

            var descAttr = builderRef.evaluator.GetType().GetCustomAttributes(typeof(ArticulateOptionsAttribute), true)
                 .OfType<ArticulateOptionsAttribute>()
                 .FirstOrDefault();
            if (descAttr != null)
            {
                description = descAttr.Description;
            }

            this.workflowManager.AddEvaluator(new LogicDefine.Evaluator() { Id = evaluatorId.Id, Description = description });
            bool isRoot = this.epxressionStack.Count == 0;

            if (!this.evaluators.ContainsKey(evaluatorId.Id))
            {
                this.evaluators.Add(evaluatorId.Id, builderRef.evaluator);
            }                //if attmpeting to add another implementation with the same id, throw an exception
                             //we can't handle this
            else if (this.evaluators[evaluatorId.Id].GetType() != builderRef.evaluator.GetType())
            {
                throw new BuilderException(evaluatorId.Id);
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

 

    }
}

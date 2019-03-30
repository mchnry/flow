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

    #region Fluent

    /// <summary>
    /// Interface for fluent activity builder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFluentActivityBuilder<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        IFluentActivityBuilder<T> Do(Action<IActionBuilder<T>> builder);
        IFluentActivityBuilder<T> DoNothing();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="If">Builder for conditional</param>
        /// <param name="Then">Builder for action</param>
        /// <returns></returns>
        IFluentElseActivityBuilder<T> IfThenDo(Action<IFluentExpressionBuilder<T>> If, Action<IFluentActivityBuilder<T>> Then);

    }
    public interface IFluentElseActivityBuilder<T>
    {
        IFluentActivityBuilder<T> Do(Action<IActionBuilder<T>> builder);
        IFluentElseActivityBuilder<T> IfThenDo(Action<IFluentExpressionBuilder<T>> If, Action<IFluentActivityBuilder<T>> Then);
        IFluentActivityBuilder<T> Else(Action<IFluentActivityBuilder<T>> Then);

    }

    public interface IFluentExpressionBuilder<T>
    {
        void True(Action<IRuleBuilder<T>> builder);
        void And(Action<IFluentExpressionBuilder<T>> first, Action<IFluentExpressionBuilder<T>> second);
        void Or(Action<IFluentExpressionBuilder<T>> first, Action<IFluentExpressionBuilder<T>> second);

    }

    public interface IActionBuilder<T>
    {
        void DoWithContext(Mchnry.Flow.Work.IAction<T> action, string context);
        void Do(Mchnry.Flow.Work.IAction<T> action);

    }

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

    public interface IRuleConditionBuilder
    {
        void IsTrue();
        void IsFalse();
        void Is(bool condition);

    }


    public interface IRuleBuilder<T>
    {
        IRuleConditionBuilder Eval(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator);
        IRuleConditionBuilder EvalWithContext(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator, string context);

    }

    public class RuleBuilder<T> : IRuleBuilder<T>, IRuleConditionBuilder
    {

        internal LogicDefine.Rule rule { get; set; }
        internal Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator { get; set; }

        public IRuleConditionBuilder Eval(IRuleEvaluator<T> evaluator)
        {
            this.evaluator = evaluator;
            this.rule = new LogicDefine.Rule() { Id = this.evaluator.Definition.Id, TrueCondition = true };
            return this;
        }
        public IRuleConditionBuilder EvalWithContext(Mchnry.Flow.Logic.IRuleEvaluator<T> evaluator, string context)
        {
            this.Eval(evaluator);
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


        void And(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);
        void Or(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);

    }

    #endregion


    public interface IBuilder<T>
    {
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
        internal string LastEquationid;

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

            LastEquationid = this.epxressionStack.Pop().RuleIdWithContext;
            
            parent.Reactions.Add(new WorkDefine.Reaction() { Logic = LastEquationid, Work = toBuild.Id });

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

            LastEquationid = this.epxressionStack.Pop().RuleIdWithContext;

            parent.Reactions.Add(new WorkDefine.Reaction() { Logic = LastEquationid, Work = toBuild.Id });

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

             string equationId = "!" + LastEquationid;           
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

            string equationId = "!" + LastEquationid;
            Then(this);



            parent.Reactions.Add(new WorkDefine.Reaction() { Logic = equationId, Work = toBuild.Id });

            this.activityStack.Pop();

            return this;
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
        }

        void IFluentExpressionBuilder<T>.True(Action<IRuleBuilder<T>> builder)
        {

            RuleBuilder<T> builderRef = new RuleBuilder<T>();
            builder.Invoke(builderRef);

            LogicDefine.Rule evaluatorId = builderRef.rule;

           

            evaluatorId.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Evaluator, evaluatorId.Id, this.config.Convention);
            this.workflowManager.AddEvaluator(new LogicDefine.Evaluator() { Id = evaluatorId.Id, Description = "Builder" });
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

        void IExpressionBuilder.True(LogicDefine.Rule evaluatorId)
        {

            evaluatorId.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Evaluator, evaluatorId.Id, this.config.Convention);
            this.workflowManager.AddEvaluator(new LogicDefine.Evaluator() { Id = evaluatorId.Id, Description = "Builder" });
            bool isRoot = this.epxressionStack.Count == 0;

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


        //IReactionBuilder IFluentActivityBuilder.Do()
        //{
        //    need to pull the parent activity off of stack and build on its id
        //    WorkDefine.Activity parent = this.activityStack.Peek();

        //    int subCount = this.subActivities[parent.Id];
        //    subCount++;
        //    this.subActivities[parent.Id] = subCount;

        //    string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
        //    WorkDefine.Activity toBuild = new WorkDefine.Activity()
        //    {
        //        Id = activityId
        //    };

        //    this.subActivities.Add(activityId, 0);
        //    activityStack.Push(toBuild);

        //    this.workflowManager.AddActivity(toBuild);


        //    return this;
        //}

        //IReactionBuilder IFluentActivityBuilder.Do(Action<IActionBuilder> DoFirst)
        //{
        //    DoFirst(this);
        //    //need to pull the parent activity off of stack and build on its id
        //    WorkDefine.Activity parent = this.activityStack.Peek();

        //    int subCount = this.subActivities[parent.Id];
        //    subCount++;
        //    this.subActivities[parent.Id] = subCount;

        //    string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
        //    WorkDefine.Activity toBuild = new WorkDefine.Activity()
        //    {
        //        Id = activityId,
        //        Action = this.created
        //    };

        //    this.subActivities.Add(activityId, 0);
        //    activityStack.Push(toBuild);

        //    this.workflowManager.AddActivity(toBuild);

        //    return this;
        //}

        //IReactionBuilder IMainActivityBuilder.Do(string activityId) { return ((IFluentActivityBuilder)this).Do(activityId); }
        //IReactionBuilder IFluentActivityBuilder.Do(string activityId)
        //{
        //    activityId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, activityId, this.config.Convention);
        //    WorkDefine.Activity toBuild = new WorkDefine.Activity() {
        //        Id = activityId
        //    };

        //    this.subActivities.Add(activityId, 0);
        //    activityStack.Push(toBuild);

        //    this.workflowManager.AddActivity(toBuild);

        //    return this;
        //}

        //IReactionBuilder IMainActivityBuilder.Do(string activityId, Action<IActionBuilder> DoFirst) { return ((IFluentActivityBuilder)this).Do(activityId, DoFirst); }
        //IReactionBuilder IFluentActivityBuilder.Do(string activityId, Action<IActionBuilder> DoFirst)
        //{
        //    DoFirst(this);
        //    activityId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, activityId, this.config.Convention);
        //    WorkDefine.Activity toBuild = new WorkDefine.Activity()
        //    {
        //        Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, activityId, this.config.Convention),
        //        Action = this.created
        //    };
        //    this.created = null;

        //    this.subActivities.Add(activityId, 0);
        //    activityStack.Push(toBuild);

        //    this.workflowManager.AddActivity(toBuild);

        //    return this;

        //}

        //IReactionBuilder IReactionBuilder.ThenAction(Action<IFluentExpressionBuilder> If, Action<IActionBuilder> Then)
        //{
        //    WorkDefine.Activity inBuild = activityStack.Peek();


        //    if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }


        //    If(this);
        //    Then(this);

        //    string lastEquationId = this.epxressionStack.Pop().RuleIdWithContext;
        //    //string lastActivityId = this.activityStack.Pop().Id;

        //    inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = this.created.ToString() });
        //    this.created = null;
        //    //no need to pop the last activity since we didn't create a new child activity.

        //    return this;
        //}

        //IReactionBuilder IReactionBuilder.ThenActivity(Action<IFluentExpressionBuilder> If, Action<IFluentActivityBuilder> Then)
        //{
        //    WorkDefine.Activity inBuild =  activityStack.Peek();

        //    if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

        //    If(this);
        //    Then(this);

        //    string lastEquationId = this.epxressionStack.Pop().RuleIdWithContext;
        //    string lastActivityId = this.activityStack.Pop().Id;

        //    inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = lastActivityId });
        //    return this;
        //}

        //IReactionBuilder IReactionBuilder.ThenAction(Action<IActionBuilder> Then)
        //{



        //    WorkDefine.Activity inBuild = activityStack.Peek();

        //    if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

        //    Then(this);


        //    inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work =  this.created.ToString() });
        //    this.created = null;
        //    return this;
        //}

        //IReactionBuilder IReactionBuilder.ThenActivity(Action<IFluentActivityBuilder> Then)
        //{
        //    WorkDefine.Activity inBuild = activityStack.Peek();

        //    if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }
        //    Then(this);

        //    string lastActivityId = this.activityStack.Pop().Id;

        //    inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work = lastActivityId });
        //    return this;
        //}


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Configuration;
using System.Linq;

namespace Mchnry.Flow
{

    public interface IMainActivityBuilder
    {
        /// <summary>
        /// Create the main activity
        /// </summary>
        /// <param name="activityId">Identifies the activity</param>
        /// <returns></returns>
        IReactionBuilder Do(string activityId);

        /// <summary>
        /// create the main activity 
        /// </summary>
        /// <param name="activityId">Identifies the activity</param>
        /// <param name="DoFirst">The actions to execute first, before evaluating any reactions</param>
        /// <returns></returns>
        IReactionBuilder Do(string activityId, Action<IActionBuilder> DoFirst);

    }

    public interface IActivityBuilder
    {
        IReactionBuilder Do();
        IReactionBuilder Do(Action<IActionBuilder> DoFirst);
        IReactionBuilder Do(string activityId);
        IReactionBuilder Do(string activityId, Action<IActionBuilder> DoFirst);

    }

    public interface IActionBuilder
    {
        void Do(WorkDefine.ActionRef action);
    }

    public interface IReactionBuilder
    {
        IReactionBuilder ThenAction(Action<IExpressionBuilder> If, Action<IActionBuilder> Then);
        IReactionBuilder ThenActivity(Action<IExpressionBuilder> If, Action<IActivityBuilder> Then);
        IReactionBuilder ThenAction(Action<IActionBuilder> Then);
        IReactionBuilder ThenActivity(Action<IActivityBuilder> Then);
        WorkDefine.Workflow End();
    }

    public interface IExpressionBuilder
    {
        void True(LogicDefine.Rule evaluatorId);


        void And(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);
        void Or(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);

    }

    public class Builder : IActivityBuilder, IReactionBuilder, IExpressionBuilder, IActionBuilder, IMainActivityBuilder
    {

        internal WorkflowManager workflowManager;

        internal Stack<WorkDefine.Activity> activityStack = new Stack<WorkDefine.Activity>();
        internal Stack<LogicDefine.IExpression> epxressionStack = new Stack<LogicDefine.IExpression>();
        internal Config config = new Config();
        internal WorkDefine.ActionRef created = null;
        internal Dictionary<string, int> subActivities = new Dictionary<string, int>();

        public static IMainActivityBuilder CreateBuilder()
        {
            return new Builder();
        }

        public static IMainActivityBuilder CreateBuilder(Action<Configuration.Config> configure)
        {
            return new Builder(configure);
        }

        internal Builder()
        {
            WorkDefine.Workflow workflow = new WorkDefine.Workflow()
            {
                Actions = new List<WorkDefine.ActionDefinition>(),
                Activities = new List<WorkDefine.Activity>(),
                Equations = new List<LogicDefine.Equation>(),
                Evaluators = new List<LogicDefine.Evaluator>()
            };
            this.workflowManager = new WorkflowManager(workflow);
        }
        public Builder (Action<Configuration.Config> configure): this()
        {
            configure(this.config);
        }

        public WorkDefine.Workflow End()
        {
            return this.workflowManager.WorkFlow;
        }


        IReactionBuilder IActivityBuilder.Do()
        {
            //need to pull the parent activity off of stack and build on its id
            WorkDefine.Activity parent = this.activityStack.Peek();

            int subCount = this.subActivities[parent.Id];
            subCount++;
            this.subActivities[parent.Id] = subCount;

            string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = activityId
            };

            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);

            this.workflowManager.AddActivity(toBuild);
   

            return this;
        }

        IReactionBuilder IActivityBuilder.Do(Action<IActionBuilder> DoFirst)
        {
            DoFirst(this);
            //need to pull the parent activity off of stack and build on its id
            WorkDefine.Activity parent = this.activityStack.Peek();

            int subCount = this.subActivities[parent.Id];
            subCount++;
            this.subActivities[parent.Id] = subCount;

            string activityId = string.Format("{0}{1}{2}", parent.Id, this.config.Convention.Delimeter, subCount);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = activityId,
                Action = this.created
            };

            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);

            this.workflowManager.AddActivity(toBuild);

            return this;
        }

        IReactionBuilder IMainActivityBuilder.Do(string activityId) { return ((IActivityBuilder)this).Do(activityId); }
        IReactionBuilder IActivityBuilder.Do(string activityId)
        {
            activityId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, activityId, this.config.Convention);
            WorkDefine.Activity toBuild = new WorkDefine.Activity() {
                Id = activityId
            };

            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);

            this.workflowManager.AddActivity(toBuild);

            return this;
        }

        IReactionBuilder IMainActivityBuilder.Do(string activityId, Action<IActionBuilder> DoFirst) { return ((IActivityBuilder)this).Do(activityId, DoFirst); }
        IReactionBuilder IActivityBuilder.Do(string activityId, Action<IActionBuilder> DoFirst)
        {
            DoFirst(this);
            activityId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, activityId, this.config.Convention);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, activityId, this.config.Convention),
                Action = this.created
            };
            this.created = null;

            this.subActivities.Add(activityId, 0);
            activityStack.Push(toBuild);

            this.workflowManager.AddActivity(toBuild);

            return this;

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
            second(this);
            secondId = this.epxressionStack.Pop().RuleIdWithContext;
            firstId = this.epxressionStack.Pop().RuleIdWithContext;

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
            second(this);
            secondId = this.epxressionStack.Pop().RuleIdWithContext;
            firstId = this.epxressionStack.Pop().RuleIdWithContext;

            toAdd.First = firstId;
            toAdd.Second = secondId;
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
            } else
            {
                this.epxressionStack.Push(evaluatorId);
            }
            //if root... then create euqations
            //otherwise, just use as evaluator
            
            


        }

     
        IReactionBuilder IReactionBuilder.ThenAction(Action<IExpressionBuilder> If, Action<IActionBuilder> Then)
        {
            WorkDefine.Activity inBuild = activityStack.Peek();
            

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

            
            If(this);
            Then(this);

            string lastEquationId = this.epxressionStack.Pop().RuleIdWithContext;
            //string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = this.created.ToString() });
            this.created = null;
            //no need to pop the last activity since we didn't create a new child activity.

            return this;
        }

        IReactionBuilder IReactionBuilder.ThenActivity(Action<IExpressionBuilder> If, Action<IActivityBuilder> Then)
        {
            WorkDefine.Activity inBuild =  activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

            If(this);
            Then(this);

            string lastEquationId = this.epxressionStack.Pop().RuleIdWithContext;
            string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = lastActivityId });
            return this;
        }

        IReactionBuilder IReactionBuilder.ThenAction(Action<IActionBuilder> Then)
        {

            

            WorkDefine.Activity inBuild = activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

            Then(this);


            inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work =  this.created.ToString() });
            this.created = null;
            return this;
        }

        IReactionBuilder IReactionBuilder.ThenActivity(Action<IActivityBuilder> Then)
        {
            WorkDefine.Activity inBuild = activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }
            Then(this);

            string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work = lastActivityId });
            return this;
        }

        void IActionBuilder.Do(WorkDefine.ActionRef DoFirst)
        {
            DoFirst.Id = ConventionHelper.EnsureConvention(NamePrefixOptions.Action, DoFirst.Id, this.config.Convention);
            this.workflowManager.AddAction(new WorkDefine.ActionDefinition() { Id = DoFirst.Id, Description = "Builder" });
            this.created = DoFirst;
        }
    }
}

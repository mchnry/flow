using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Configuration;
using System.Linq;

namespace Mchnry.Flow
{


    public interface IActivityBuilder
    {
        IReactionBuilder Activity(string activityId);
        IReactionBuilder Activity(string activityId, WorkDefine.ActionRef actionId);

    }
    public interface IReactionBuilder
    {
        IReactionBuilder Then(WorkDefine.ActionRef actionId, Action<IExpressionBuilder> If);
        IReactionBuilder Then(Action<IActivityBuilder> activity, Action<IExpressionBuilder> If);
        IReactionBuilder Then(WorkDefine.ActionRef actionId);
        IReactionBuilder Then(Action<IActivityBuilder> activity);
        WorkDefine.Workflow End();
    }

    public interface IExpressionBuilder
    {
        void True(LogicDefine.Rule evaluatorId);


        void And(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);
        void Or(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);

    }

    public class Builder : IActivityBuilder, IReactionBuilder, IExpressionBuilder
    {

        internal WorkDefine.Workflow workflow = null;

        internal Stack<WorkDefine.Activity> activityStack = new Stack<WorkDefine.Activity>();
        internal Stack<LogicDefine.IExpression> epxressionStack = new Stack<LogicDefine.IExpression>();
        internal Config config = new Config();

        public Builder()
        {
            this.workflow = new WorkDefine.Workflow()
            {
                Actions = new List<WorkDefine.ActionDefinition>(),
                Activities = new List<WorkDefine.Activity>(),
                Equations = new List<LogicDefine.Equation>(),
                Evaluators = new List<LogicDefine.Evaluator>()
            };
        }
        public Builder (Action<Configuration.Config> configure): this()
        {
            configure(this.config);
        }

        public WorkDefine.Workflow End()
        {
            return this.workflow;
        }

        public IReactionBuilder Activity(string activityId)
        {
            WorkDefine.Activity toBuild = new WorkDefine.Activity() {
                Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Activity, activityId, this.config.Convention)
            };


            activityStack.Push(toBuild);
            
            workflow.Activities.Add(toBuild);
            
            return this;
        }

        public IReactionBuilder Activity(string activityId, WorkDefine.ActionRef actionId)
        {

            actionId.Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Action, actionId.Id, this.config.Convention);
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Activity, activityId, this.config.Convention),
                Action = actionId
            };


            activityStack.Push(toBuild);

            workflow.Activities.Add(toBuild);

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
            if (this.workflow.Equations.Count(g => g.Id == equationId) == 0)
            {
                this.workflow.Equations.Add(toAdd);
            }

            string firstId, secondId = null;
            first(this);
            second(this);
            secondId = this.epxressionStack.Pop().Id;
            firstId = this.epxressionStack.Pop().Id;

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
            if (this.workflow.Equations.Count(g => g.Id == equationId) == 0)
            {
                this.workflow.Equations.Add(toAdd);
            }

            string firstId, secondId = null;
            first(this);
            second(this);
            secondId = this.epxressionStack.Pop().Id;
            firstId = this.epxressionStack.Pop().Id;

            toAdd.First = firstId;
            toAdd.Second = secondId;
        }

        void IExpressionBuilder.True(LogicDefine.Rule evaluatorId)
        {

            evaluatorId.Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Evaluator, evaluatorId.Id, this.config.Convention);

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

                if (this.workflow.Equations.Count(g => g.Id == equationId) == 0)
                {

                    this.workflow.Equations.Add(toAdd);
                }
            } else
            {
                this.epxressionStack.Push(evaluatorId);
            }
            //if root... then create euqations
            //otherwise, just use as evaluator
            
            


        }

     
        IReactionBuilder IReactionBuilder.Then(WorkDefine.ActionRef actionId, Action<IExpressionBuilder> If)
        {
            WorkDefine.Activity inBuild = activityStack.Peek();
            actionId.Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Action, actionId.Id, this.config.Convention);

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

            
            If(this);

            string lastEquationId = this.epxressionStack.Pop().Id;
            //string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = actionId.ToString() });
            
            //no need to pop the last activity since we didn't create a new child activity.

            return this;
        }

        IReactionBuilder IReactionBuilder.Then(Action<IActivityBuilder> activity, Action<IExpressionBuilder> If)
        {
            WorkDefine.Activity inBuild =  activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

            activity(this);
            If(this);
            

            string lastEquationId = this.epxressionStack.Pop().Id;
            string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = lastActivityId });
            return this;
        }

        IReactionBuilder IReactionBuilder.Then(WorkDefine.ActionRef actionId)
        {

            actionId.Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Action, actionId.Id, this.config.Convention);

            WorkDefine.Activity inBuild = activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }



            inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work =  actionId.ToString() });
            return this;
        }

        IReactionBuilder IReactionBuilder.Then(Action<IActivityBuilder> activity)
        {
            WorkDefine.Activity inBuild = activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }
            activity(this);

            string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work = lastActivityId });
            return this;
        }


    }
}

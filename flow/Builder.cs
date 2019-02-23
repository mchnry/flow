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
        IReactionBuilder Do(string activityId);
        IReactionBuilder Do(string activityId, Action<IActionBuilder> doFirst);

    }

    public interface IActionBuilder
    {
        void Do(WorkDefine.ActionRef action);
    }

    public interface IReactionBuilder
    {
        IReactionBuilder ThenAction(Action<IExpressionBuilder> If, Action<IActionBuilder> action);
        IReactionBuilder ThenActivity(Action<IExpressionBuilder> If, Action<IActivityBuilder> activity);
        IReactionBuilder ThenAction(Action<IActionBuilder> action);
        IReactionBuilder ThenActivity(Action<IActivityBuilder> activity);
        WorkDefine.Workflow End();
    }

    public interface IExpressionBuilder
    {
        void True(LogicDefine.Rule evaluatorId);


        void And(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);
        void Or(Action<IExpressionBuilder> first, Action<IExpressionBuilder> second);

    }

    public class Builder : IActivityBuilder, IReactionBuilder, IExpressionBuilder, IActionBuilder
    {

        internal WorkDefine.Workflow workflow = null;

        internal Stack<WorkDefine.Activity> activityStack = new Stack<WorkDefine.Activity>();
        internal Stack<LogicDefine.IExpression> epxressionStack = new Stack<LogicDefine.IExpression>();
        internal Config config = new Config();
        internal WorkDefine.ActionRef created = null;


        public static IActivityBuilder CreateBuilder()
        {
            return new Builder();
        }

        internal Builder()
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

        IReactionBuilder IActivityBuilder.Do(string activityId)
        {
            WorkDefine.Activity toBuild = new WorkDefine.Activity() {
                Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Activity, activityId, this.config.Convention)
            };


            activityStack.Push(toBuild);
            
            workflow.Activities.Add(toBuild);
            
            return this;
        }

        IReactionBuilder IActivityBuilder.Do(string activityId, Action<IActionBuilder> DoFirst)
        {

            
            WorkDefine.Activity toBuild = new WorkDefine.Activity()
            {
                Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Activity, activityId, this.config.Convention),
                Action = this.created
            };
            this.created = null;

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
            if (this.workflow.Equations.Count(g => g.Id == equationId) == 0)
            {
                this.workflow.Equations.Add(toAdd);
            }

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

     
        IReactionBuilder IReactionBuilder.ThenAction(Action<IExpressionBuilder> If, Action<IActionBuilder> Then)
        {
            WorkDefine.Activity inBuild = activityStack.Peek();
            

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

            
            If(this);

            string lastEquationId = this.epxressionStack.Pop().RuleIdWithContext;
            //string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = this.created.ToString() });
            this.created = null;
            //no need to pop the last activity since we didn't create a new child activity.

            return this;
        }

        IReactionBuilder IReactionBuilder.ThenActivity(Action<IExpressionBuilder> If, Action<IActivityBuilder> activity)
        {
            WorkDefine.Activity inBuild =  activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }

            activity(this);
            If(this);
            

            string lastEquationId = this.epxressionStack.Pop().RuleIdWithContext;
            string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() { Logic = lastEquationId, Work = lastActivityId });
            return this;
        }

        IReactionBuilder IReactionBuilder.ThenAction(Action<IActionBuilder> action)
        {

            

            WorkDefine.Activity inBuild = activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }



            inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work =  this.created.ToString() });
            this.created = null;
            return this;
        }

        IReactionBuilder IReactionBuilder.ThenActivity(Action<IActivityBuilder> activity)
        {
            WorkDefine.Activity inBuild = activityStack.Peek();

            if (inBuild.Reactions == null) { inBuild.Reactions = new List<WorkDefine.Reaction>(); }
            activity(this);

            string lastActivityId = this.activityStack.Pop().Id;

            inBuild.Reactions.Add(new WorkDefine.Reaction() {  Work = lastActivityId });
            return this;
        }

        void IActionBuilder.Do(WorkDefine.ActionRef action)
        {
            action.Id = ConventionHelper.ApplyConvention(NamePrefixOptions.Action, action.Id, this.config.Convention);
            this.created = action;
        }
    }
}

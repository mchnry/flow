using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Configuration;

namespace Mchnry.Flow
{
    internal class WorkflowManager
    {
        private readonly Configuration.Config config;

        internal WorkDefine.Workflow WorkFlow { get; set; }

        public WorkflowManager(WorkDefine.Workflow workFlow, Configuration.Config config)
        {
            this.WorkFlow = workFlow;
            this.config = config;
        }

        internal virtual void RenameWorkflow(string newName)
        {
            string orig = WorkFlow.Id;
            string mainActivity = this.RootActivityId;

            string newId = newName + this.config.Convention.Delimeter + "Main";
            this.WorkFlow.Id = newName;

            this.WorkFlow.Equations.ForEach(a =>
            {
                a.Id = a.Id.Replace(mainActivity, newId);
            });

            this.WorkFlow.Activities.ForEach(a =>
            {
                a.Id = a.Id.Replace(mainActivity, newId);
                if (a.Reactions != null)
                {
                    a.Reactions.ForEach(r =>
                    {
                        if (!string.IsNullOrEmpty(r.Logic)) r.Logic = r.Logic.Replace(mainActivity, newId);
                        r.Work = r.Work.Replace(mainActivity, newId);
                    });
                }
            });

        }

        public virtual WorkDefine.Activity GetActivity(string id)
        {
            WorkDefine.Activity definition = this.WorkFlow.Activities.FirstOrDefault(a => a.Id == id);
            return definition;
        }
        public virtual WorkDefine.ActionDefinition GetActionDefinition(string id)
        {
            WorkDefine.ActionDefinition match = this.WorkFlow.Actions.FirstOrDefault(z => z.Id == id);
            return match;
        }

        public virtual LogicDefine.Equation GetEquation(string id)
        {
            LogicDefine.Equation eq = this.WorkFlow.Equations.FirstOrDefault(g => g.Id.Equals(id));
            return eq;
        }
        public virtual LogicDefine.Evaluator GetEvaluator(string id)
        {
            LogicDefine.Evaluator ev = this.WorkFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(id));
            return ev;
        }

        public virtual void AddActivity(WorkDefine.Activity toAdd)
        {
            WorkDefine.Activity match = this.GetActivity(toAdd.Id);
            if (match == null)
            {
                this.WorkFlow.Activities.Add(toAdd);
            } 
        }
        public virtual void AddAction(WorkDefine.ActionDefinition toAdd)
        {
            WorkDefine.ActionDefinition match = this.GetActionDefinition(toAdd.Id);
            if (match == null)
            {
                this.WorkFlow.Actions.Add(toAdd);
            }
        }
        public virtual void AddEquation(LogicDefine.Equation toAdd)
        {
            LogicDefine.Equation match = this.GetEquation(toAdd.Id);
            if (match == null)
            {
                this.WorkFlow.Equations.Add(toAdd);
            } 
        }
        public virtual void AddEvaluator(LogicDefine.Evaluator toAdd)
        {
            LogicDefine.Evaluator match = this.GetEvaluator(toAdd.Id);
            if (match == null)
            {
                this.WorkFlow.Evaluators.Add(toAdd);
            }
        }
        public virtual ContextDefinition GetContextDefinition(string name)
        {
            return this.WorkFlow.ContextDefinitions.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        public virtual void AddContextDefinition(ContextDefinition toAdd)
        {
            ContextDefinition match = this.GetContextDefinition(toAdd.Name);
            if (match == null)
            {
                this.WorkFlow.ContextDefinitions.Add(toAdd);
            }
        }

        public virtual String RootActivityId 
        {
            get {
                string toReturn = this.WorkFlow.Id + this.config.Convention.Delimeter + "Main";
                return toReturn;
            }
        }
    }
}

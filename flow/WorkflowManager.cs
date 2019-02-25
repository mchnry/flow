using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow
{
    internal class WorkflowManager
    {
        internal WorkDefine.Workflow WorkFlow { get; set; }

        public WorkflowManager(WorkDefine.Workflow workFlow)
        {
            this.WorkFlow = workFlow;
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

        public virtual List<String> GetRootActivities()
        {
            Dictionary<string, int> activityRefs = new Dictionary<string, int>();

            Action<string> findRefs = null;
            findRefs = (a) =>
            {
                if (!activityRefs.ContainsKey(a)) { activityRefs.Add(a, 0); }
                else
                {
                    activityRefs[a] += 1;
                }
                WorkDefine.Activity reffed = this.GetActivity(a);
                if (reffed != null)
                {
                    if (reffed.Reactions != null && reffed.Reactions.Count() > 0)
                    {
                        reffed.Reactions.ForEach(r =>
                        {
                            WorkDefine.ActionRef workRef = r.Work;
                            if (this.GetActivity(workRef.Id) != null)
                            {
                                findRefs(workRef.Id);
                            }
                        });
                    }
                }
            };
            this.WorkFlow.Activities.ForEach(g =>
            {
                findRefs(g.Id);
            });

            List<String> rootActivities = (from rootActivity in activityRefs where rootActivity.Value == 0 select rootActivity.Key).ToList();
            return rootActivities;

        }
    }
}

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
    }
}

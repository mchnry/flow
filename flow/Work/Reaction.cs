using Mchnry.Flow.Work.Define;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Work
{
    internal class Reaction<TModel>
    {
        public Reaction(string logicEquationId, Activity<TModel> activity)
        {
            this.LogicEquationId = logicEquationId;
            this.Activity = activity;
        }

        public Reaction(string logicEquationId, ActionRef action)
        {
            this.LogicEquationId = logicEquationId;
            this.Action = action;

        }

        public string LogicEquationId { get; }
        public Activity<TModel> Activity { get; }
        public ActionRef Action { get; }

        public bool Processed { get; set; } = false;
    }
}

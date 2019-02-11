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

        public string LogicEquationId { get; }
        public Activity<TModel> Activity { get; }

        public bool Processed { get; set; } = false;
    }
}

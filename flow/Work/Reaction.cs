using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Work
{
    internal class Reaction
    {
        public Reaction(string logicEquationId, Activity activity)
        {
            this.LogicEquationId = logicEquationId;
            this.Activity = activity;
        }

        public string LogicEquationId { get; }
        public Activity Activity { get; }

        public bool Processed { get; set; } = false;
    }
}

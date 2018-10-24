using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Work.Define
{
    public sealed class Workflow
    {

        public Workflow()
        {
            this.Evaluators = new List<Evaluator>();
            this.Equations = new List<Equation>();
            this.Actions = new List<ActionDefinition>();
            this.Activities = new List<Activity>();

        }
        public List<Evaluator> Evaluators { get; set; }
        public List<Equation> Equations { get; set; }
        public List<ActionDefinition> Actions { get; set; }
        public List<Activity> Activities { get; set; }
    }
}

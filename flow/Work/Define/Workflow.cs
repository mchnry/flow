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
        }
        public List<Evaluator> Evaluators { get; set; } = new List<Evaluator>();
        public List<Equation> Equations { get; set; } = new List<Equation>();
        public List<ActionDefinition> Actions { get; set; } = new List<ActionDefinition>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
    } 
}

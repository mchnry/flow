using System.Collections.Generic;
using Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow.Work.Define
{
    public sealed class Workflow
    {

        public Workflow(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
        public string LintHash { get; set; }

        public List<ContextDefinition> ContextDefinitions { get; set; }

        public List<Evaluator> Evaluators { get; set; } = new List<Evaluator>();
        public List<Equation> Equations { get; set; } = new List<Equation>();
        public List<ActionDefinition> Actions { get; set; } = new List<ActionDefinition>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
    } 
}

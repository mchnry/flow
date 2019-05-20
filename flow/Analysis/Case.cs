using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mchnry.Flow.Analysis
{
    public class Case : ICloneable
    {
        internal Case(List<Rule> rules)
        {
      
            this.Rules = rules;
            this.HitAndRuns = new List<HitAndRun>();
        }

        public List<StepTraceNode<ActivityProcess>> Trace { get; internal set; }
        public List<HitAndRun> HitAndRuns { get; }


        public int Id { get;set; }

        public List<Rule> Rules { get; set; } = new List<Rule>();

        public object Clone()
        {

            return new Case((from r in this.Rules select (Rule)r.Clone()).ToList());
        }
    }
}

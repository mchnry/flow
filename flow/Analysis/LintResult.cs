using Mchnry.Flow.Diagnostics;
using System.Collections.Generic;

namespace Mchnry.Flow.Analysis
{
    public class LintResult
    {
        internal LintResult(StepTracer<LintTrace> tracer, List<ActivityTest> activityTests, string lintHash)
        {
            this.Trace = tracer;
            this.ActivityTests = activityTests;
            this.LintHash = LintHash;
        }

        public string LintHash { get; }
        public StepTracer<LintTrace> Trace { get; }
        public List<ActivityTest> ActivityTests { get; }

    }
}

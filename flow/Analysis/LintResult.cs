using Mchnry.Flow.Diagnostics;
using System.Collections.Generic;

namespace Mchnry.Flow.Analysis
{
    public class LintResult
    {
        internal LintResult(StepTracer<LintTrace> tracer, List<ActivityTest> surfaceTests, List<ActivityTest> mockTests, List<Audit> auditResults, string lintHash)
        {
            this.Trace = tracer;
            this.SurfaceTests = surfaceTests;
            this.MockTests = mockTests;
            this.AuditResults = auditResults;
            this.LintHash = LintHash;
        }

        public string LintHash { get; }
        public StepTracer<LintTrace> Trace { get; }
        public List<ActivityTest> SurfaceTests { get; }
        public List<ActivityTest> MockTests { get; }
        public List<Audit> AuditResults { get; }
    }
}

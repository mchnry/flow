﻿using Mchnry.Flow.Diagnostics;
using System.Collections.Generic;

namespace Mchnry.Flow.Analysis
{
    public class LintResult
    {
        internal LintResult(StepTracer<LintTrace> tracer, List<LogicTest> logicTests, string lintHash)
        {
            this.Trace = tracer;
            this.LogicTests = logicTests;
            this.LintHash = LintHash;
        }

        public string LintHash { get; }
        public StepTracer<LintTrace> Trace { get; }
        public List<LogicTest> LogicTests { get; }

    }
}
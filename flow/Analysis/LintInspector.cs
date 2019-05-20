using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Configuration;

namespace Mchnry.Flow.Analysis
{
    public class LintInspector
    {
        private readonly WorkDefine.Workflow workflow;
        private readonly Configuration.Config configuration;

        public LintInspector(LintResult result, WorkDefine.Workflow workflow, Configuration.Config configuration)
        {
            this.Result = result;
            this.workflow = workflow;
            this.configuration = configuration;
        }

        public ArticulateActivity ArticulateActivity(string activityId)
        {

            activityId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, activityId, this.configuration.Convention);

            Articulator articulator = new Articulator( this.workflow, this.configuration);
            return articulator.ArticulateActivity(activityId, false, false);

        }

        public List<ArticulateEvaluator> ArticulateTestCase(ActivityTest test, int testCaseId)
        {
            Articulator articulator = new Articulator(this.workflow, this.configuration);
            return articulator.ArticulateTestCase(test, testCaseId, false, false);
        }

        public LintResult Result { get; }
    }
}

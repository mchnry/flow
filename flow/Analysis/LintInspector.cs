using Mchnry.Flow.Configuration;
using System.Collections.Generic;
using WorkDefine = Mchnry.Flow.Work.Define;

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

        public ArticulateActivity ArticulateFlow()
        {


            Articulator articulator = new Articulator( this.workflow, this.configuration);
            return articulator.ArticulateFlow(false, false);

        }

        public List<ArticulateEvaluator> ArticulateTestCase(ActivityTest test, int testCaseId)
        {
            Articulator articulator = new Articulator(this.workflow, this.configuration);
            return articulator.ArticulateTestCase(test, testCaseId, false, false);
        }

        public LintResult Result { get; }
    }
}

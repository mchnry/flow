using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow.Analysis
{
    public class Articulator
    {
        private readonly List<LogicIntent> logicIntents;
        private readonly WorkDefine.Workflow workflow;
        private readonly Configuration.Config configuration;

        public Articulator(List<LogicIntent> logicIntents, WorkDefine.Workflow workflow, Configuration.Config configuration)
        {
            this.logicIntents = logicIntents;
            this.workflow = workflow;
            this.configuration = configuration;
        }


        public string ArticulateActivity(string activityId, bool removeConvention, bool verbose)
        {
            string toReturn = string.Empty;



            return toReturn;
        }
        public string ArticulateTestCase(ActivityTest test, int caseId, bool removeConvention, bool verbose)
        {
            string toReturn = string.Empty;


            return toReturn;
        }

        
    }
}

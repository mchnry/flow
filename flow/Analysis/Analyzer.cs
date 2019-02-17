using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using System.Linq;

namespace Mchnry.Flow.Analysis
{
    internal class CaseAnalyzer
    {
        private readonly WorkDefine.Workflow workflow;
        private readonly List<ActivityTest> surfaceTests;
        private readonly List<ActivityTest> mockTests;

        public CaseAnalyzer(WorkDefine.Workflow workflow, List<ActivityTest> surfaceTests) : this(workflow, surfaceTests, null) { }
        public CaseAnalyzer(WorkDefine.Workflow workflow, List<ActivityTest> surfaceTests, List<ActivityTest> mockTests)
        {
            this.workflow = workflow;
            this.surfaceTests = surfaceTests;
            this.mockTests = mockTests;
        }


        public List<Audit> Analyze()
        {
            List<Audit> toReturn = new List<Audit>();

            CalcuateHitAndRuns(this.surfaceTests);
            if (this.mockTests != null)
            {
                CalcuateHitAndRuns(this.mockTests);
            }

            //any case where no action run
            //any evaluator that is never run
            //any action that is never run
            //mismatch on mock and surface counts


           

            return toReturn;

        }

        internal void CalcuateHitAndRuns(List<ActivityTest> tests)
        {
            tests.ForEach(t =>
            {
                t.TestCases.ForEach(tc =>
                {
                    foreach (LogicDefine.Evaluator ev in this.workflow.Evaluators)
                    {
                        HitAndRun toAdd = new HitAndRun(ev.Id);

                        toAdd.HitCount = (from u in tc.Trace
                                          where u.Node.Value.ProcessId == ev.Id
                                          && (u.Node.Value.Status == ActivityStatusOptions.Rule_Evaluating ||
                                            u.Node.Value.Status == ActivityStatusOptions.Rule_NotRun_Cached ||
                                            u.Node.Value.Status == ActivityStatusOptions.Rule_NotRun_ShortCircuit 
                                          ) select u).Count();
                        toAdd.RunCount = (from u in tc.Trace
                                          where u.Node.Value.ProcessId == ev.Id
                                          && (u.Node.Value.Status == ActivityStatusOptions.Rule_Evaluating 
                                          )
                                          select u).Count();

                        tc.HitAndRuns.Add(toAdd);
                    }
                    foreach (WorkDefine.ActionDefinition ad in this.workflow.Actions)
                    {
                        HitAndRun toAdd = new HitAndRun(ad.Id);

                        toAdd.HitCount = (from u in tc.Trace
                                          where u.Node.Value.ProcessId == ad.Id
                                          && (u.Node.Value.Status == ActivityStatusOptions.Action_Running 
                                          )
                                          select u).Count();
                        toAdd.RunCount = (from u in tc.Trace
                                          where u.Node.Value.ProcessId == ad.Id
                                          && (u.Node.Value.Status == ActivityStatusOptions.Action_Running
                                          )
                                          select u).Count();

                        tc.HitAndRuns.Add(toAdd);
                    }


                });
            });
        }

    }
}

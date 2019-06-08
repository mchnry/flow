using System.Collections.Generic;
using System.Linq;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Analysis
{
    public enum AuditCode
    {
        ActionNeverRun,
        NoActionRunInTestCase,
        EvaluatorNeverRun
    }

    internal class CaseAnalyzer
    {
        private readonly WorkflowManager workflowManager;
        private readonly List<ActivityTest> surfaceTests;
        private readonly List<ActivityTest> mockTests;
        private readonly Configuration.Config configuration;

        public CaseAnalyzer(WorkflowManager workflowManager, List<ActivityTest> surfaceTests, Configuration.Config configuration) : this(workflowManager, surfaceTests, null, configuration) { }
        public CaseAnalyzer(WorkflowManager workflowManager, List<ActivityTest> surfaceTests, List<ActivityTest> mockTests, Configuration.Config configuration)
        {
            this.workflowManager = workflowManager;
            this.surfaceTests = surfaceTests;
            this.mockTests = mockTests;
            this.configuration = configuration;
        }


        public List<Audit> Analyze()
        {
            List<Audit> toReturn = new List<Audit>();

            CalculateHitAndRuns(this.surfaceTests);
            if (this.mockTests != null)
            {
                CalculateHitAndRuns(this.mockTests);
            }

            //aggregate hitandruns
            List<HitAndRun> aggregate = AggregateHitAndRuns(this.surfaceTests);

            //any action that is never run
            var neverRunActions = (from a in this.workflowManager.WorkFlow.Actions
                                   join b in aggregate on a.Id equals b.Id
                                   where b.RunCount == 0
                                   select a);
            if (neverRunActions.Count() > 0)
            {
                toReturn.AddRange(from a in neverRunActions select new Audit(
                    AuditCode.ActionNeverRun,
                    AuditSeverity.Critical, a.Id, "Action is never run"));
            }
            //any evaluator that is never run
            var neverRunEvaluators = (from a in this.workflowManager.WorkFlow.Evaluators
                                   join b in aggregate on a.Id equals b.Id
                                   where b.RunCount == 0
                                   select a);
            if (neverRunEvaluators.Count() > 0)
            {
                toReturn.AddRange(from a in neverRunEvaluators select new Audit(
                    AuditCode.EvaluatorNeverRun,
                    AuditSeverity.Critical, a.Id, "Evaluator is never run"));
            }

            string actionConvention = string.Format("{0}{1}", this.configuration.Convention.GetPrefix(Configuration.NamePrefixOptions.Action), this.configuration.Convention.Delimeter);
            string root = this.workflowManager.RootActivityId;
            //any case where no action is run (exclude placeholder)
            foreach (ActivityTest at in this.surfaceTests.Where(t => t.ActivityId == root))
            {
                
                foreach (Case testCase in at.TestCases)
                {

                    

                    int actionRuns = (from z in testCase.HitAndRuns where z.Id.StartsWith(actionConvention) select z).Aggregate(0, (a, b) =>
                    {
                        return a + b.RunCount;
                    });
                    if (actionRuns == 0)
                    {
                        toReturn.Add(new Audit(
                            AuditCode.NoActionRunInTestCase,
                            AuditSeverity.Critical, testCase.Id.ToString(), string.Format("Test Case for Activity {0} did not run any actions", at.ActivityId)));
                    }
                }
            }

            //mismatch on mock and surface counts
            //activities with no reactions



            return toReturn;

        }

        internal void CalculateHitAndRuns(List<ActivityTest> tests)
        {
            tests.ForEach(t =>
            {
                t.TestCases.ForEach(tc =>
                {
                    foreach (LogicDefine.Evaluator ev in this.workflowManager.WorkFlow.Evaluators)
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
                    foreach (WorkDefine.ActionDefinition ad in this.workflowManager.WorkFlow.Actions)
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

        internal List<HitAndRun> AggregateHitAndRuns(List<ActivityTest> tests)
        {
            List<HitAndRun> toReturn = new List<HitAndRun>();
   
            tests.ForEach(t =>
            {
                t.TestCases.ForEach(tc =>
                {
                    foreach (LogicDefine.Evaluator ev in this.workflowManager.WorkFlow.Evaluators)
                    {

                        
                        
                        HitAndRun toAdd = toReturn.FirstOrDefault(i => i.Id == ev.Id);
                        if (toAdd == null) 
                        {
                            toAdd = new HitAndRun(ev.Id);
                            toReturn.Add(toAdd);
                        }
                        HitAndRun fromTC = tc.HitAndRuns.First(h => h.Id == ev.Id);
                        toAdd.HitCount += fromTC.HitCount;
                        toAdd.RunCount += fromTC.RunCount;

                        
                    }
                    foreach (WorkDefine.ActionDefinition ad in this.workflowManager.WorkFlow.Actions)
                    {
                        HitAndRun toAdd = toReturn.FirstOrDefault(i => i.Id == ad.Id);
                        if (toAdd == null)
                        {
                            toAdd = new HitAndRun(ad.Id);
                            toReturn.Add(toAdd);
                        }
                        HitAndRun fromTC = tc.HitAndRuns.First(h => h.Id == ad.Id);
                        toAdd.HitCount += fromTC.HitCount;
                        toAdd.RunCount += fromTC.RunCount;



                    }


                });
            });

            return toReturn;
        }
    }
}

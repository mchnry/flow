using Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Work.Define;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mchnry.Flow.Analysis
{

    internal class Linter: INeedIntent {

        private List<LogicTest> logicTests = null;
        internal Dictionary<string, List<Case>> EquationTests { get; set; } = new Dictionary<string, List<Case>>();
        private List<string> lefts = null;
        private List<string> rights = null;
        private List<string> roots = null;

        internal Linter(WorkflowManager workflowManager)
        {
            this.WorkflowManager = workflowManager;

            //infer intents
            this.lefts = (from e in this.WorkflowManager.WorkFlow.Equations
                          where e.First != null
                          select e.First.Id).ToList();

            this.rights = (from e in this.WorkflowManager.WorkFlow.Equations
                           where null != e.Second
                           select e.Second.Id).ToList();

            this.roots = (from e in this.WorkflowManager.WorkFlow.Equations
                          where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
                          select e.Id).ToList();



            this.InferIntent();

        }

        public WorkflowManager WorkflowManager { get; }

        internal void InferIntent()
        {
            List<string> evalIds = (from e in this.WorkflowManager.WorkFlow.Evaluators select e.Id).ToList();
            List<Rule> evalRules = (from l in this.WorkflowManager.WorkFlow.Equations
                                    where evalIds.Contains(l.First.Id)
                                    select l.First)
                                    .Union(from r in this.WorkflowManager.WorkFlow.Equations
                                           where null != r.Second
                                           && evalIds.Contains(r.Second.Id)
                                           select r.Second).ToList();
            List<string> hasContext = (from r in evalRules where !string.IsNullOrEmpty(r.Context) select r.Id).Distinct().ToList();
            hasContext.ForEach(x =>
            {
                List<ContextItem> options = (from r in evalRules where r.Id == x select new ContextItem() { Key = r.Context, Literal = "Inferred" }).Distinct().ToList();
                LogicIntent toAdd = new LogicIntent(x);
                toAdd.HasContext().HasValues(options).OneOfExcusive();
                this.Intents.Add(toAdd);
            });
        }


        internal List<LogicIntent> Intents { get; set; } = new List<LogicIntent>();

        public LogicIntent Intent(string evaluatorId)
        {
            LogicIntent match = this.Intents.FirstOrDefault(g => g.evaluatorId == evaluatorId);
            LogicIntent toAdd = default(LogicIntent);
            if (match != null)
            {
                toAdd = match;
            }
            else
            {
                toAdd = new LogicIntent(evaluatorId);
                this.Intents.Add(toAdd);
            }

            return toAdd;
        }



        public List<LogicTest> LogicLint()
        {

            if (this.logicTests != null)
            {
                return this.logicTests;
            }
            else
            {

                List<LogicTest> toReturn = new List<LogicTest>();


                //loop through roots
                Func<List<Case>, List<Rule>, int, List<Case>> buildCases = null;
                buildCases = (childCases, rules, ordinal) =>
                {
                    List<Case> resolved = new List<Case>();
                    new bool[] { true, false }.ToList().ForEach(t =>
                    {
                        Rule conditional = (Rule)rules[ordinal].Clone();
                        conditional.TrueCondition = t;

                        if (childCases != null)
                        {
                            childCases.ForEach(c =>
                            {
                                Case cloned = (Case)c.Clone();
                                Case toAdd = new Case( cloned.Rules);
                                toAdd.Rules.Add(conditional);
                                resolved.Add(toAdd);
                            });
                        }
                        else
                        {
                            resolved.Add(new Case(new List<Rule>() { conditional }));
                        }


                    });

                    if (ordinal < (rules.Count - 1))
                    {
                        resolved = buildCases(resolved, rules, ordinal + 1);
                    }

                    return resolved;

                };

                Func<Equation, List<Rule>> ExtractRules = null;
                ExtractRules = (s) =>
                {
                    List<Rule> extracted = new List<Rule>();
                    if (null != s.First)
                    {
                        Equation qMatch = this.WorkflowManager.WorkFlow.Equations.FirstOrDefault(g => g.Id.Equals(s.First.Id));
                    //if first is another euqation, extract it's rules
                    if (null != qMatch)
                        {
                            List<Rule> fromEq = ExtractRules(qMatch);
                            extracted.AddRange(fromEq);
                        }
                        else //otherwise, get the rule
                    {

                            if (s.First.Id != "true")
                            {

                                extracted.Add(s.First);
                            }
                        }

                    }
                    if (null != s.Second)
                    {
                        Equation qMatch = this.WorkflowManager.WorkFlow.Equations.FirstOrDefault(g => g.Id.Equals(s.Second.Id));
                    //if first is another euqation, extract it's rules
                    if (null != qMatch)
                        {
                            List<Rule> fromEq = ExtractRules(qMatch);
                            extracted.AddRange(fromEq);
                        }
                        else //otherwise, get the rule
                    {
                            if (s.Second.Id != "true")
                            {
                                extracted.Add(s.Second);
                            }
                        }

                    }

                    return extracted;
                };

                List<string> intentRules = (from i in this.Intents select i.evaluatorId).ToList();
                this.roots.ForEach(r =>
                {
                    List<List<Case>> SubCases = new List<List<Case>>();
                    Equation toTest = this.WorkflowManager.WorkFlow.Equations.FirstOrDefault(g => g.Id.Equals(r));
                    List<Rule> equationRules = ExtractRules(toTest);

                //build cases for no-intent
                List<Rule> noIntent = (from z in equationRules where !intentRules.Contains(z.Id) select z).ToList();

                    if (noIntent.Count > 0)
                    {
                        List<Case> noIntentCases = buildCases(null, noIntent, 0);
                        SubCases.Add(noIntentCases);
                    }

                    if (this.Intents.Count > 0)
                    {
                        List<string> myEvals = (from i in equationRules select i.Id).Distinct().ToList();
                        List<LogicIntent> myIntents = (from i in this.Intents where myEvals.Contains(i.evaluatorId) select i).ToList();

                        myIntents.ForEach(i =>
                        {
                            List<Rule> myIntentRules = (from e in equationRules where e.Id == i.evaluatorId select e).ToList();
                            List<Case> intentCases = buildCases(null, myIntentRules, 0);

                        //if intent is oneOf, get rid of any cases where more than one rule is true
                        if (i.Context.ListType == ValidateOptions.OneOf)
                            {
                                intentCases.RemoveAll((c) => c.Rules.Count(t => t.TrueCondition) > 1);
                            }
                        //if exclusive, get rid of all false case
                        if (i.Context.Exclusive)
                            {
                                intentCases.RemoveAll((c) => c.Rules.Count(t => !t.TrueCondition) == c.Rules.Count());
                            }
                            SubCases.Add(intentCases);
                        });



                    }

                    LogicTest equationTest = new LogicTest(r, true);
                    if (SubCases.Count > 1)
                    {
                        List<Case> merged = new List<Case>((from c in SubCases[0] select (Case)c.Clone()));


                    //merge subcases
                    for (int i = 1; i < SubCases.Count; i++)
                        {

                            List<Case> toMerge = SubCases[i];
                            List<Case> thisMerge = new List<Case>();

                            for (int j = 0; j < merged.Count; j++)
                            {
                                for (int q = 0; q < toMerge.Count; q++)
                                {
                                    Case cloneMerged = (Case)merged[j].Clone();
                                    Case cloneThis = (Case)toMerge[q].Clone();
                                    cloneMerged.Rules.AddRange(cloneThis.Rules);
                                    Case toAdd = new Case(cloneMerged.Rules);
                                    thisMerge.Add(toAdd);
                                }
                            }
                            merged = thisMerge;

                        }

                        equationTest.TestCases = merged;
                    }
                    else if (SubCases.Count == 1)
                    {
                        equationTest.TestCases = SubCases[0];
                    }
                    else
                    {
                        equationTest.TestCases = new List<Case>();
                    }


                    toReturn.Add(equationTest);

                });


                return this.logicTests = toReturn;
            }
        }
        public List<ActivityTest> AcvityLint()
        {


            //make sure logictest was run
            var runLogicTests = this.LogicLint();


            /*for each root activity
            need to get all equations involved
            merge those test cases from top down without adding duplicates on EvaluatorID|context 
            example
             if (a & b) -> X -> 
                  if !a & c -> y
            result should be
                a, b, c (not a,!a,b,c)

            */
            List<ActivityTest> toReturn = new List<ActivityTest>();

            Dictionary<string, int> activityRefs = new Dictionary<string, int>();

            Action<string> findRefs = null;
            findRefs = (a) =>
            {
                if (!activityRefs.ContainsKey(a)) { activityRefs.Add(a, 0); }
                else
                {
                    activityRefs[a] += 1;
                }
                Activity reffed = this.WorkflowManager.GetActivity(a);
                if (reffed != null)
                {
                    if (reffed.Reactions != null && reffed.Reactions.Count() > 0)
                    {
                        reffed.Reactions.ForEach(r =>
                        {
                            ActionRef workRef = r.Work;
                            if (this.WorkflowManager.GetActivity(workRef.Id) != null)
                            {
                                findRefs(workRef.Id);
                            }
                        });
                    }
                }
            };
            this.WorkflowManager.WorkFlow.Activities.ForEach(g =>
            {
                findRefs(g.Id);
            });

            List<String> rootActivities = (from rootActivity in activityRefs where rootActivity.Value == 0 select rootActivity.Key).ToList();

            rootActivities.ForEach((rootActivity) =>
            {
                List<string> equationsWithCases = new List<string>();
                equationsWithCases = (from z in logicTests where z.TestCases.Count > 0 select z.EquationId).ToList();
                List<List<string>> cases = new List<List<string>>();

                foreach (string eq in equationsWithCases)
                {


                    LogicTest eqTest = logicTests.First(g => g.EquationId == eq);
                    List<List<string>> thisRun = new List<List<string>>();
                    foreach (Case testCase in eqTest.TestCases)
                    {
                        List<string> ruleCases = (from g in testCase.Rules select g.ToString()).ToList();
                        if (cases.Count == 0)
                        {
                            thisRun.Add(ruleCases);
                        }
                        else
                        {
                            foreach (List<string> lastRun in cases)
                            {
                                List<string> copyOfLastRun = (from s in lastRun select s).ToList();
                                //whittle out any dupes where id and context match. 
                                List<string> justIdsFromLastRun = (from s in lastRun select ((Rule)s).RuleIdWithContext).ToList();

                                List<string> deduped = (from s in ruleCases where !justIdsFromLastRun.Contains(((Rule)s).RuleIdWithContext) select s).ToList();

                                List<string> thisCase = copyOfLastRun;

                                if (deduped.Count > 0)
                                {
                                    thisCase = copyOfLastRun.Union(deduped).ToList();
                                    thisRun.Add(thisCase);
                                }

                            }
                        }

                    }
                    if (thisRun.Count > 0)
                    {
                        cases = thisRun;
                    }


                }

                List<Case> testCases = (from z in cases select new Case((from x in z select (Rule)x).ToList())).ToList();

                ActivityTest toAdd = new ActivityTest(rootActivity) { TestCases = testCases };
                toReturn.Add(toAdd);


            });


            int counter = 0;
            toReturn.ForEach(t =>
            {
                t.TestCases.ForEach(c => { counter++; c.Id = counter; });
            });

            return toReturn;

        }
    }

    
}

using System;
using System.Collections.Generic;
using System.Text;
using Mchnry.Flow.Logic.Define;
using System.Linq;

namespace Mchnry.Flow.Test
{
    public class LogicLinter
    {
        private readonly List<Evaluator> evaluatorDefinitions;
        private readonly List<Equation> equationDefinitions;

        private List<string> lefts = null;
        private List<string> rights = null;
        private List<string> roots = null;


        internal Dictionary<string, List<Case>> EquationTests { get; set; } = new Dictionary<string, List<Case>>();

        internal LogicLinter(List<Evaluator> evaluatorDefinitions,
            List<Equation> equationDefinitions)
        {
            this.evaluatorDefinitions = evaluatorDefinitions;
            this.equationDefinitions = equationDefinitions;
            //infer intents
            this.lefts = (from e in this.equationDefinitions
                                  where e.First != null
                                  select e.First.Id).ToList();

            this.rights = (from e in this.equationDefinitions
                                   where null != e.Second
                                   select e.Second.Id).ToList();

            this.roots = (from e in this.equationDefinitions
                                  where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
                                  select e.Id).ToList();

            

            this.InferIntent();
        }


        internal void InferIntent()
        {
            List<string> evalIds = (from e in this.evaluatorDefinitions select e.Id).ToList();
            List<Rule> evalRules = (from l in this.equationDefinitions
                                    where evalIds.Contains(l.First.Id)
                                    select l.First)
                                    .Union(from r in this.equationDefinitions
                                           where null != r.Second
                                           && evalIds.Contains(r.Second.Id)
                                           select r.Second).ToList();
            List<string> hasContext = (from r in evalRules where !string.IsNullOrEmpty(r.Context) select r.Id).Distinct().ToList();
            hasContext.ForEach(x =>
            {
                List<string> options = (from r in evalRules where r.Id == x select r.Context).Distinct().ToList();
                LogicIntent toAdd = new LogicIntent(x);
                toAdd.HasContext<string>().HasValues(options).OneOfExcusive();
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

        public List<LogicTest> Lint()
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
                              Case toAdd = new Case(cloned.Rules);
                              toAdd.Rules.Add(conditional);
                              resolved.Add(toAdd);
                          });
                      } else
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
                    Equation qMatch = this.equationDefinitions.FirstOrDefault(g => g.Id.Equals(s.First.Id));
                    //if first is another euqation, extract it's rules
                    if (null != qMatch)
                    {
                        List<Rule> fromEq = ExtractRules(qMatch);
                        extracted.AddRange(fromEq);
                    }
                    else //otherwise, get the rule
                    {
                        extracted.Add(s.First);
                    }
                    
                }
                if (null != s.Second)
                {
                    Equation qMatch = this.equationDefinitions.FirstOrDefault(g => g.Id.Equals(s.Second.Id));
                    //if first is another euqation, extract it's rules
                    if (null != qMatch)
                    {
                        List<Rule> fromEq = ExtractRules(qMatch);
                        extracted.AddRange(fromEq);
                    }
                    else //otherwise, get the rule
                    {
                        extracted.Add(s.Second);
                    }

                }

                return extracted;
            };

            List<string> intentRules = (from i in this.Intents select i.evaluatorId).ToList();
            this.roots.ForEach(r =>
            {
                List<List<Case>> SubCases = new List<List<Case>>();
                Equation toTest = this.equationDefinitions.FirstOrDefault(g => g.Id.Equals(r));
                List<Rule> equationRules = ExtractRules(toTest);

                //build cases for no-intent
                List<Rule> noIntent = (from z in equationRules where !intentRules.Contains(z.Id) select z).ToList();
                List<Case> noIntentCases = buildCases(null, noIntent, 0);
                SubCases.Add(noIntentCases);

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

                LogicTest equationTest = new LogicTest(r);
                if (SubCases.Count > 1)
                {
                    List<Case> merged = new List<Case>((from c in SubCases[0] select (Case)c.Clone()));


                    //merge subcases
                    for (int i = 1; i < SubCases.Count; i ++)
                    {
                        
                        List<Case> toMerge = SubCases[i];
                        List<Case> thisMerge = new List<Case>();

                        for (int j = 0; j < merged.Count; j++)
                        {
                            for (int q = 0; q < toMerge.Count; q ++)
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
                } else
                {
                    equationTest.TestCases = SubCases[0];
                }
                toReturn.Add(equationTest);

            });

            return toReturn;

        }
       

    }

    internal class ActivityLinter
    {
        //what logic cases result in reaction
        //any activities where no reaction is executed



    }
}

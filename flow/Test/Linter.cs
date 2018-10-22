using System;
using System.Collections.Generic;
using System.Text;
using Mchnry.Flow.Logic.Define;
using System.Linq;

namespace Mchnry.Flow.Test
{
    public class Linter
    {
        private readonly List<Evaluator> evaluatorDefinitions;
        private readonly List<Equation> equationDefinitions;

        private List<string> lefts = null;
        private List<string> rights = null;
        private List<string> roots = null;


        internal Dictionary<string, List<Case>> EquationTests { get; set; } = new Dictionary<string, List<Case>>();

        internal Linter(List<Evaluator> evaluatorDefinitions,
            List<Equation> equationDefinitions)
        {
            this.evaluatorDefinitions = evaluatorDefinitions;
            this.equationDefinitions = equationDefinitions;
            //infer intents
            this.lefts = (from e in this.equationDefinitions
                                  where !string.IsNullOrEmpty(e.First.Id)
                                  select e.First.Id).ToList();

            this.rights = (from e in this.equationDefinitions
                                   where !string.IsNullOrEmpty(e.Second.Id)
                                   select e.Second.Id).ToList();

            this.roots = (from e in this.equationDefinitions
                                  where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
                                  select e.Id).ToList();

            this.InferIntent();
        }


        private void InferIntent()
        {
            List<string> evalIds = (from e in this.evaluatorDefinitions select e.Id).ToList();
            List<Rule> evalRules = (from l in this.equationDefinitions
                                    where !string.IsNullOrEmpty(l.First.Id)
                                    && evalIds.Contains(l.First.Id)
                                    select l.First)
                                    .Union(from r in this.equationDefinitions
                                           where !string.IsNullOrEmpty(r.Second.Id)
                                           && evalIds.Contains(r.Second.Id)
                                           select r.Second).ToList();
            List<string> hasContext = (from r in evalRules where !string.IsNullOrEmpty(r.Context) select r.Id).Distinct().ToList();
            hasContext.ForEach(x =>
            {
                List<string> options = (from r in evalRules where r.Id == x select r.Context).Distinct().ToList();
                Intent toAdd = new Intent(x);
                toAdd.HasContext<string>().HasValues(options).OneOfExcusive();
                this.Intents.Add(toAdd);
            });
        }


        internal List<Intent> Intents { get; set; } = new List<Intent>();

        public Intent Intent(string evaluatorId)
        {
            Intent match = this.Intents.FirstOrDefault(g => g.evaluatorId == evaluatorId);
            Intent toAdd = default(Intent);
            if (match != null)
            {
                toAdd = match;
            }
            else
            {
                toAdd = new Intent(evaluatorId);
                this.Intents.Add(toAdd);
            }
            
            return toAdd;
        }

        public ValidationContainer Lint()
        {
            ValidationContainer toReturn = new ValidationContainer();

            List<Rule> NoIntent = new List<Rule>();
            List<List<Rule>> WithIntent = new List<List<Rule>>();

            //loop through roots
            Func<Case, List<Rule>, int, Case> travers = null;
            travers = (childCase, rules, ordinal) =>
            {
                Case resolved = new Case();
                new bool[] { true, false }.ToList().ForEach(t =>
                  {
                      Rule conditional = rules[ordinal];
                      conditional.TrueCondition = true;

                      if (childCase != null)
                      {
                          List<Rule> merged = (from r in childCase.Rules)
                      }

                  });
                



                return resolved;
            };

        }
       

    }
}

using Mchnry.Flow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using System.Linq;
using Mchnry.Flow.Configuration;

namespace Mchnry.Flow.Analysis
{
    public sealed class Sanitizer
    {

        StepTracer<string> tracer;
        Configuration.Config config;

        public Sanitizer(StepTracer<string> tracer, Configuration.Config config)
        {
            this.tracer = tracer;
            this.config = config;
        }

        public WorkDefine.Workflow Sanitize(WorkDefine.Workflow toSanitize)
        {
            List<string> activityIds = (from a in toSanitize.Activities select a.Id).ToList();
            foreach(string a in activityIds)
            {
                LoadActivity(toSanitize, a);
            }
            return toSanitize;
        }

        private void LoadActivity(WorkDefine.Workflow workFlow, string activityId)
        {

            WorkDefine.Activity definition = workFlow.Activities.FirstOrDefault(a => a.Id == activityId);

            
            Action<WorkDefine.Activity, bool> LoadReactions = null;
            LoadReactions = (d, isroot) =>
            {
                if (d.Action == null) d.Action = "*placeHolder";

                WorkDefine.ActionDefinition match = workFlow.Actions.FirstOrDefault(z => z.Id == d.Action.ActionId);
                if (null == match)
                {
                    workFlow.Actions.Add(new WorkDefine.ActionDefinition()
                    {
                        Id = d.Action.ActionId,
                        Description = ""
                    });
                }

                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.Activity toCreatedef = workFlow.Activities.FirstOrDefault(z => z.Id == r.Work);

                        if (null == toCreatedef)
                        {
                            //if we can't find activity... look for a matching action.  if found, create an activity from it.
                            WorkDefine.ActionRef asActionRef = r.Work;
                            WorkDefine.ActionDefinition toCreateAction = workFlow.Actions.FirstOrDefault(z => z.Id == asActionRef.ActionId);

                            //didn't bother to add the action definition, we will create it for them
                            if (null == toCreateAction)
                            {
                                workFlow.Actions.Add(new WorkDefine.ActionDefinition()
                                {
                                    Id = asActionRef.ActionId,
                                    Description = ""
                                });

                                if (isroot)
                                {
                                    string newId = ConventionHelper.ChangePrefix(NamePrefixOptions.Action, NamePrefixOptions.Activity, asActionRef.ActionId, this.config.Convention);
                                    WorkDefine.Activity toAdd = new WorkDefine.Activity()
                                    {
                                        Action = asActionRef,
                                       
                                        Id = newId,
                                        Reactions = null
                                    };
                                    workFlow.Activities.Add(toAdd);
                                    r.Work = newId;
                                }
                            }


                            toCreatedef = new WorkDefine.Activity()
                            {
                                Action = asActionRef,
                                Id = Guid.NewGuid().ToString(),
                                Reactions = new List<WorkDefine.Reaction>()
                            };

                        }

                        if (string.IsNullOrEmpty(r.Logic))
                        {
                            r.Logic = "true";
                        }

                        r.Logic = LoadLogic(workFlow, r.Logic);


                        LoadReactions(toCreatedef, false);
                 
                    });
                }

            };

            LoadReactions(definition, true);

 

        }



        //this needs to
        // * ensure reaction rule is an equation
        // * ensure that any evaluators exist in the evaluators list
        private string LoadLogic(WorkDefine.Workflow workFlow, string equationId)
        {

            StepTraceNode<string> root = this.tracer.Root;


            //load conventions

            LogicDefine.Evaluator trueDef = workFlow.Evaluators.FirstOrDefault(z => z.Id == "true");
            if (null == trueDef)
            {
                trueDef = new LogicDefine.Evaluator() { Id = "true", Description = "Always True" };
                workFlow.Evaluators.Add(trueDef);
            }


            //List<string> lefts = (from e in workFlow.Equations
            //                      where e.First != null
            //                      select e.First.Id).ToList();

            //List<string> rights = (from e in workFlow.Equations
            //                       where null != e.Second
            //                       select e.Second.Id).ToList();

            //List<string> roots = (from e in workFlow.Equations
            //                      where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
            //                      select e.Id).ToList();


            //Lint.... make sure we have everything we need first.
            Action<LogicDefine.Rule, StepTraceNode<string>, bool> LoadRule = null;
            LoadRule = (rule, parentStep, isRoot) =>
            {
                StepTraceNode<string> step = this.tracer.TraceNext(parentStep, rule.Id);
               
                //if id is an equation, we are creating an expression
                LogicDefine.Equation eq = workFlow.Equations.FirstOrDefault(g => g.Id.Equals(rule.Id));
                if (null != eq)
                {
                   
                    if (null != eq.First)
                    {
                        LoadRule(eq.First, step, false);
                    }
                    else
                    {
                        eq.First = new LogicDefine.Rule() { Id = "true", Context = string.Empty, TrueCondition = true };
                    }

                    if (null != eq.Second)
                    {
                        LoadRule(eq.Second.Id, step, false);
                    }
                    else
                    {

                        eq.Second = new LogicDefine.Rule() { Id = "true", Context = string.Empty, TrueCondition = true };
                    }
                    
                    if (!rule.TrueCondition)
                    {
                        //create a negation equation.
                        string negationId = ConventionHelper.NegateEquationName(rule.Id, this.config.Convention);
                        LogicDefine.Rule negated = (LogicDefine.Rule)rule.Clone();
                        negated.TrueCondition = false;

                        LogicDefine.Equation toAdd = new LogicDefine.Equation()
                        {
                            First = negated,
                            Id = negationId,
                            Condition = Logic.Operand.And,
                            Second = "true"
                        };
                        workFlow.Equations.Add(toAdd);
                        rule.Id = negationId;
                    }

                }
                else
                {

                    //if reaction ruleid is not an equation, create an equation and update reaction

                    LogicDefine.Evaluator ev = workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(rule.Id));

                    if (null == ev)
                    {
                        workFlow.Evaluators.Add(new LogicDefine.Evaluator()
                        {
                            Id = rule.Id,
                            Description = string.Empty
                        });

                        //if this is the rule referenced by the reaction, then create an equation also,
                        //and  update the equation.  This isn't necessary, but consistent.
                        if (isRoot)
                        {
                            LogicDefine.Rule cloned = (LogicDefine.Rule) rule.Clone();
                            string newId = ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Equation, rule.Id, this.config.Convention);
                            workFlow.Equations.Add(new LogicDefine.Equation()
                            {
                                Condition = Logic.Operand.And,
                                First = cloned,
                                Second = "true",
                                Id = newId
                            });
                            rule.Id = newId;

                        }

                    }



                }


            };




            LogicDefine.Rule eqRule = equationId;
            LoadRule(eqRule, root, true);

            return eqRule.Id;

        }


    }
}

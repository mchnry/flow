﻿using Mchnry.Flow.Configuration;
using Mchnry.Flow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Analysis
{

    /// <summary>
    /// Utility used to Sanitize a workflow definition
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>This is used to </item>
    /// </list>
    /// </remarks>
    internal sealed class Sanitizer
    {

        StepTracer<LintTrace> tracer;
        Configuration.Config config;
        List<LogicDefine.Equation> sanitized = new List<LogicDefine.Equation>();


        public Sanitizer(StepTracer<LintTrace> tracer, Configuration.Config config)
        {
            this.tracer = tracer;
            this.config = config;
        }

        public WorkDefine.Workflow Sanitize(WorkDefine.Workflow toSanitize)
        {
            List<string> activityIds = (from a in toSanitize.Activities select a.Id).ToList();
            foreach (string a in activityIds)
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

                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.Activity toCreatedef = workFlow.Activities.FirstOrDefault(z => z.Id == r.Work);

                        if (null == toCreatedef)
                        {
                            //if we can't find activity... look for a matching action.  if found, create an activity from it.
                            WorkDefine.ActionRef asActionRef = r.Work;
                            WorkDefine.ActionDefinition toCreateAction = workFlow.Actions.FirstOrDefault(z => z.Id == asActionRef.Id);

                            //didn't bother to add the action definition, we will create it for them
                            if (null == toCreateAction)
                            {
                                workFlow.Actions.Add(new WorkDefine.ActionDefinition()
                                {
                                    Id = asActionRef.Id,
                                    Description = ""
                                });

                            }


                            toCreatedef = new WorkDefine.Activity()
                            {
                                //Action = asActionRef,
                                Id = Guid.NewGuid().ToString(),
                                Reactions = new List<WorkDefine.Reaction>()
                            };

                        }

                        if (string.IsNullOrEmpty(r.Logic))
                        {
                            r.Logic = ConventionHelper.TrueEquation(this.config.Convention);
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

            StepTraceNode<LintTrace> root = this.tracer.Root;


            //load conventions

            LogicDefine.Evaluator trueDef = workFlow.Evaluators.FirstOrDefault(z => z.Id == ConventionHelper.TrueEvaluator(this.config.Convention));
            if (null == trueDef)
            {
                trueDef = new LogicDefine.Evaluator() { Id = ConventionHelper.TrueEvaluator(this.config.Convention), Description = "Always True" };
                workFlow.Evaluators.Add(trueDef);
            }
            LogicDefine.Equation trueEqDef = workFlow.Equations.FirstOrDefault(z => z.Id == ConventionHelper.TrueEquation(this.config.Convention));
            if (null == trueEqDef)
            {
                trueEqDef = new LogicDefine.Equation() { Condition = Logic.Operand.Or, First = trueDef.Id, Second = trueDef.Id, Id = ConventionHelper.TrueEquation(this.config.Convention) };
                workFlow.Equations.Add(trueEqDef);
            }



            //Lint.... make sure we have everything we need first.
            Action<LogicDefine.Rule, StepTraceNode<LintTrace>, bool> LoadRule = null;
            LoadRule = (rule, parentStep, isRoot) =>
            {
                StepTraceNode<LintTrace> step = this.tracer.TraceNext(parentStep, new LintTrace(LintStatusOptions.Inspecting, "Inspecting Rule", rule.Id));

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
                        eq.First = new LogicDefine.Rule() { Id = ConventionHelper.TrueEvaluator(this.config.Convention), Context = string.Empty, TrueCondition = true };
                    }

                    if (null != eq.Second)
                    {
                        LoadRule(eq.Second.Id, step, false);
                    }
                    else
                    {

                        eq.Second = new LogicDefine.Rule() { Id = ConventionHelper.TrueEvaluator(this.config.Convention), Context = string.Empty, TrueCondition = true };
                    }

                    if (!rule.TrueCondition)
                    {
                        //create a negation equation.
                        string negationId = ConventionHelper.NegateEquationName(rule.Id, this.config.Convention);
                        LogicDefine.Rule negated = (LogicDefine.Rule)rule.Clone();
                        //negated.TrueCondition = false;

                        if (workFlow.Equations.Count(g => g.Id == negationId) == 0)
                        {
                            this.tracer.TraceNext(parentStep, new LintTrace(LintStatusOptions.InferringEquation, string.Format("Inferring negation equation from {0}", rule.Id), negationId));
                            LogicDefine.Equation toAdd = new LogicDefine.Equation()
                            {
                                First = negated,
                                Id = negationId,
                                Condition = Logic.Operand.And,
                                Second = ConventionHelper.TrueEvaluator(this.config.Convention)
                            };
                            workFlow.Equations.Add(toAdd);

                            rule.TrueCondition = true;
                            rule.Id = negationId;
                        }


                    }

                }
                else
                {

                    //if reaction ruleid is not an equation, create an equation and update reaction

                    LogicDefine.Evaluator ev = workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(rule.Id));

                    if (null == ev)
                    {
                        this.tracer.TraceNext(parentStep, new LintTrace(LintStatusOptions.LazyDefinition, "No definition found for evaluator", rule.Id));
                        ev = new LogicDefine.Evaluator()
                        {
                            Id = rule.Id,
                            Description = string.Empty
                        };

                        workFlow.Evaluators.Add(ev);
                    }

                    //if this is the rule referenced by the reaction, then create an equation also,
                    //and  update the equation.  This isn't necessary, but consistent.
                    if (isRoot)
                    {
                        LogicDefine.Rule cloned = (LogicDefine.Rule)rule.Clone();
                        string newId = string.Empty;
                        Logic.Operand condition = Logic.Operand.And;
                        if (rule.Id == ConventionHelper.TrueEquation(this.config.Convention))
                        {
                            newId = ConventionHelper.TrueEquation(this.config.Convention);
                            condition = Logic.Operand.Or;
                        }
                        else
                        {
                            newId = ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Equation, rule.Id, this.config.Convention);
                        }

                        if (!rule.TrueCondition)
                        {
                            newId = ConventionHelper.NegateEquationName(newId, this.config.Convention);
                        }
                        if (workFlow.Equations.Count(g => g.Id == newId) == 0)
                        {
                            this.tracer.TraceNext(parentStep, new LintTrace(LintStatusOptions.InferringEquation, string.Format("Inferring equation from {0}", rule.Id), newId));
                            workFlow.Equations.Add(new LogicDefine.Equation()
                            {
                                Condition = condition,
                                First = cloned,
                                Second = ConventionHelper.TrueEvaluator(this.config.Convention),
                                Id = newId
                            });
                        }
                        rule.Id = newId;

                    }





                }


            };




            LogicDefine.Rule eqRule = equationId;
            LoadRule(eqRule, root, true);

            return eqRule.Id;

        }


    }
}

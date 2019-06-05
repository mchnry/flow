using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using System.Linq;
using Mchnry.Flow.Configuration;

namespace Mchnry.Flow.Analysis
{
    public class Articulator
    {

        private readonly WorkDefine.Workflow workflow;
        private readonly Configuration.Config configuration;

        public Articulator(WorkDefine.Workflow workflow, Configuration.Config configuration)
        {
      
            this.workflow = workflow;
            this.configuration = configuration;
        }

        private ArticulateEvaluator ArticulateEvaluator(LogicDefine.Rule x)
        {
            ArticulateEvaluator toBuild = new ArticulateEvaluator() { Id = x.Id };
            LogicDefine.Evaluator ev = this.workflow.Evaluators.FirstOrDefault(g => g.Id == x.Id);
            toBuild.Literal = ev.Description;

            toBuild.TrueCondition = x.TrueCondition;
            if (x.Context != null)
            {
                ContextDefinition ctxDef = this.workflow.ContextDefinitions.FirstOrDefault(g => g.Name.Equals(x.Context.Name, StringComparison.OrdinalIgnoreCase));

                string ctxLit = string.Join(",", (from i in ctxDef.Items where x.Context.Keys.Contains(i.Key) select i.Literal));
                
                ArticulateContext articulateContext = new ArticulateContext()
                {
                    Literal = ctxDef.Literal, Value = ctxLit
                };
                //var intent = this.logicIntents.FirstOrDefault(g => g.evaluatorId == x.Id);
                //if (intent != null && intent.Context != null)
                //{
                //    articulateContext.Literal = intent.Context.Literal;
                //    if (intent.Context.Values != null)
                //    {
                //        ContextItem match = intent.Context.Values.FirstOrDefault(k => k.Key == ctx.Key);
                //        if (!string.IsNullOrEmpty(match.Key))
                //        {
                //            ctx.Literal = match.Literal;
                //        }
                        
                //    }


                //}
                //if (string.IsNullOrEmpty(ctx.Literal)) { ctx.Literal = "Inferred"; }
                //if (string.IsNullOrEmpty(articulateContext.Literal)) { articulateContext.Literal = "Inferred"; }
                //articulateContext.Context = ctx;
                toBuild.Context = articulateContext;

            }

            return toBuild;
        }


        public ArticulateActivity ArticulateActivity(string activityId, bool removeConvention, bool verbose)
        {

            Func<WorkDefine.ActionRef, IArticulateActivity> createAction = (s) =>
            {

                var actionDef = this.workflow.Actions.FirstOrDefault(g => g.Id == s.Id);
                ArticulateAction action = new ArticulateAction() { Id = s.Id, Literal = actionDef.Description };
                

                if (s.Id == "*placeHolder")
                {
                    return new NothingAction();
                }
                else
                {

                    if (s.Input != null)
                    {

                        ArticulateContext articulateContext = new ArticulateContext()
                        {
                            Literal = "Input",
                            Value = s.Input
                        };
                        action.Context = articulateContext;
                    }

                    return action;
                }
            };

            ArticulateActivity toReturn = new ArticulateActivity();
       
            WorkDefine.Activity toArticulate = this.workflow.Activities.FirstOrDefault(g => g.Id == activityId);
            toReturn.Id = toArticulate.Id;




            Func<LogicDefine.Rule, IArticulateExpression> traverseExpression = null;
            traverseExpression = (x) =>
            {
                IArticulateExpression buildExpression = null;

               
                if (ConventionHelper.MatchesConvention(NamePrefixOptions.Evaluator, x.Id, this.configuration.Convention))
                {
                    

                    buildExpression = this.ArticulateEvaluator(x);

                } else
                {
                    ArticulateExpression toBuild = new ArticulateExpression() { Id = x.Id };
                    LogicDefine.Equation eq = this.workflow.Equations.FirstOrDefault(g => g.Id == x.Id);
                    toBuild.Condition = (eq.Condition == Logic.Operand.And) ? "and": "or";
                    toBuild.First = traverseExpression(eq.First);
                    toBuild.Second = traverseExpression(eq.Second);
                    buildExpression = toBuild;
                }


                return buildExpression;
            };

            Action<ArticulateActivity, WorkDefine.Activity> traverseActivity = null;
            traverseActivity = (a, d) =>
            {
                if (d.Reactions != null && d.Reactions.Count() > 0)
                {
                    a.Reactions = new List<ArticulateReaction>();
                    d.Reactions.ForEach(r =>
                    {
                        //all logic at this point should be equations
                        //if logic = true, then if = "Always".
                        ArticulateReaction toAdd = new ArticulateReaction();
                        if (r.Logic == ConventionHelper.TrueEquation(this.configuration.Convention))
                        {
                            toAdd.If = new TrueExpression();
                        } else
                        {
                            toAdd.If = traverseExpression(r.Logic);
                        }

                        WorkDefine.ActionRef aref = r.Work;
                        if (ConventionHelper.MatchesConvention(NamePrefixOptions.Action, aref.Id, this.configuration.Convention)) {
                            toAdd.Then = createAction(aref);
                        } else
                        {
                            WorkDefine.Activity toTraverse = this.workflow.Activities.FirstOrDefault(g => g.Id == aref.Id);
                            ArticulateActivity Then = new ArticulateActivity() { Id = aref.Id };
                            traverseActivity(Then, toTraverse);
                            toAdd.Then = Then;
                        }
                        a.Reactions.Add(toAdd);
                        
                    });
                }
            };

            traverseActivity(toReturn, toArticulate);


            return toReturn;
        }
        public List<ArticulateEvaluator> ArticulateTestCase(ActivityTest test, int caseId, bool removeConvention, bool verbose)
        {
            List<ArticulateEvaluator> toReturn = new List<ArticulateEvaluator>();


            Case toArticulate = test.TestCases.FirstOrDefault(c => c.Id == caseId);

            toArticulate.Rules.ForEach(r =>
            {
                toReturn.Add(this.ArticulateEvaluator(r));
            });

            

            return toReturn;
        }

        
    }
}

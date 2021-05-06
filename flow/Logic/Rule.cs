using Mchnry.Flow.Diagnostics;
using Mchnry.Flow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Mchnry.Flow.Logic
{
    public class Rule<TModel> : IRule<TModel>
    {

        private readonly Define.Rule definition;
        private Engine<TModel> engineRef;
        private readonly bool inner;

        internal Rule(
            
            Define.Rule definition,
            Engine<TModel> EngineRef,
            bool inner
            )
        {

            this.definition = definition;
            this.engineRef = EngineRef;
            this.inner = inner;
        }

        public bool Inner { get => this.inner; }
        public string Id => this.definition.Id;

        
        public string RuleIdWithContext => this.definition.RuleIdWithContext;
        public string ShortHand => this.definition.ShortHand;

        public async Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token)
        {

            bool thisResult = !this.definition.TrueCondition;
            
            bool? knownResult = this.engineRef.RunManager.GetResult(this.definition);
            IRuleEvaluatorX<TModel> evaluator = this.engineRef.ImplementationManager.GetEvaluator(
                new Define.Evaluator() { Id = this.definition.Id });

            bool doEval = reEvaluate || !knownResult.HasValue;

            this.engineRef.RunManager.CurrentRuleDefinition = this.definition;
            this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_Evaluating;

            Action<bool, Validation> status = (a, s) =>
             {

                 if (s !=null)
                 {
                     if (s.Severity == ValidationSeverity.Confirm)
                     {
                         var oride = engineRef.ValidationContainer.Overrides.FirstOrDefault(g => g.Key.EndsWith(s.Key, StringComparison.OrdinalIgnoreCase));
                         if (oride == null)
                         {
                             thisResult = a;
                             engineRef.AddValidation(s);
                         } else
                         {
                             thisResult = true;
                         }

                     } else
                     {
                         thisResult = a;
                         engineRef.AddValidation(s);
                     }
                 } else
                 {
                     thisResult = a;
                 }
             };
            
            if (doEval)
            {
                //#TODO implement metric in evaluation
                try
                {

                    StepTraceNode<ActivityProcess> mark = this.engineRef.Tracer.CurrentStep = this.engineRef.Tracer.TraceStep(
                        new ActivityProcess(this.definition.Id, ActivityStatusOptions.Rule_Evaluating, null));

                    Stopwatch t = new Stopwatch(); t.Start();

                    //if evaluator, run and return result
                    if (evaluator is IEvaluatorRule<TModel>)
                    {
                        bool result = await ((IEvaluatorRule<TModel>)evaluator).EvaluateAsync(this.engineRef, new LogicEngineTrace(this.engineRef.Tracer), token);
                        status(result, null);
                    } else
                    {
                        await ((IValidatorRule<TModel>)evaluator).ValidateAsync(
                            this.engineRef,
                            new LogicEngineTrace(this.engineRef.Tracer),
                            new RuleResult(status),
                            token
                            );
                            
                    }

                    t.Stop();
                    this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_Evaluated;

                    this.engineRef.Tracer.TraceStep(new ActivityProcess(this.definition.Id, ActivityStatusOptions.Rule_Evaluated, null, t.Elapsed));

                }
                catch (EvaluateException ex)
                {
                    this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_Failed;
                    this.engineRef.Tracer.TraceStep(new ActivityProcess(this.definition.Id, ActivityStatusOptions.Rule_Failed, ex.Message));
                    throw new EvaluateException(this.definition.Id, this.definition.Context.ToString(), ex);
                }
                // Cache stores the evaluator results only
                this.engineRef.RunManager.SetResult(this.definition, thisResult);
                knownResult = thisResult;


            } else
            {
                this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_NotRun_Cached;
                this.engineRef.Tracer.TraceStep(new ActivityProcess(this.definition.Id, ActivityStatusOptions.Rule_NotRun_Cached, null));

            }

            return (knownResult.Value == this.definition.TrueCondition);
        }
    }
}

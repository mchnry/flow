using Mchnry.Flow.Diagnostics;
using Mchnry.Flow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public class Rule<TModel> : IRule<TModel>
    {

        private readonly Define.Rule definition;
        private Engine<TModel> engineRef;

        internal Rule(
            
            Define.Rule definition,
            Engine<TModel> EngineRef
            )
        {

            this.definition = definition;
            this.engineRef = EngineRef;
        }

        public async Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token)
        {

            bool thisResult = !this.definition.TrueCondition;

            bool? knownResult = this.engineRef.RunManager.GetResult(this.definition);
            IRuleEvaluator<TModel> evaluator = this.engineRef.ImplementationManager.GetEvaluator(this.definition.Id);

            bool doEval = reEvaluate || !knownResult.HasValue;

            this.engineRef.RunManager.CurrentRuleDefinition = this.definition;
            this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_Evaluating;

            StepTraceNode<ActivityProcess> mark =  this.engineRef.Tracer.CurrentStep = this.engineRef.Tracer.TraceStep(
                new ActivityProcess(this.definition.Id, ActivityStatusOptions.Rule_Evaluating, null));
            
            if (doEval)
            {
                //#TODO implement metric in evaluation
                try
                {
                    Stopwatch t = new Stopwatch(); t.Start();

                    thisResult = await evaluator.EvaluateAsync(
                        this.engineRef,
                        new LogicEngineTrace(this.engineRef.Tracer),
                        token);
                    t.Stop();
                    this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_Evaluated;

                    this.engineRef.Tracer.TraceStep(new ActivityProcess(this.definition.Id, ActivityStatusOptions.Rule_Evaluated, null, t.Elapsed));

                }
                catch (EvaluateException ex)
                {
                    this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_Failed;
                    this.engineRef.Tracer.TraceStep(new ActivityProcess(this.definition.Id, ActivityStatusOptions.Rule_Failed, ex.Message));
                    throw new EvaluateException(this.definition.Id, this.definition.Context, ex);
                }
                // Cache stores the evaluator results only
                this.engineRef.RunManager.SetResult(this.definition, thisResult);
                knownResult = thisResult;


            } else
            {
                this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Rule_NotRun_Cached;
            }

            return (knownResult.Value == this.definition.TrueCondition);
        }
    }
}

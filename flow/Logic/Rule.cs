using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Exception;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public class Rule : IRule
    {

        private readonly Define.Rule definition;
        private readonly IRuleEngine engineRef;

        internal Rule(
            
            Define.Rule definition,
            IRuleEngine EngineRef
            )
        {

            this.definition = definition;
            this.engineRef = EngineRef;
        }

        public async Task<bool> EvaluateAsync(bool reEvaluate, IStepTracer<string> tracer, CancellationToken token)
        {

            bool thisResult = !this.definition.TrueCondition;

            bool? knownResult = this.engineRef.GetResult(this.definition);
            IRuleEvaluator evaluator = this.engineRef.GetEvaluator(this.definition.Id);

            bool doEval = reEvaluate || !knownResult.HasValue;



            if (doEval)
            {
                //#TODO implement metric in evaluation
                try
                {
                    thisResult = await evaluator.EvaluateAsync(
                        this.definition,
                        engineRef.CurrentProcessId,
                        this.engineRef.State,
                        this.engineRef.GetContainer(this.definition),
                        tracer,
                        token);

                    
                }
                catch (EvaluateException ex)
                {
                    throw new EvaluateException(this.definition.Id, this.definition.Context, ex);
                }
                // Cache stores the evaluator results only
                this.engineRef.SetResult(this.definition, thisResult);
                knownResult = thisResult;


            }

            return (knownResult.Value == this.definition.TrueCondition);
        }
    }
}

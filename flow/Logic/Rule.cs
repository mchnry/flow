using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public class Rule : IRule
    {
        private readonly bool trueCondition;
        private readonly string context;
        private readonly Define.Evaluator definition;
        private readonly IRuleEngine engineRef;

        internal Rule(
            bool trueCondition, 
            string context,
            Define.Evaluator definition,
            IRuleEngine EngineRef
            )
        {
            this.trueCondition = trueCondition;
            this.context = context;
            this.definition = definition;
            this.engineRef = EngineRef;
        }

        public async Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token)
        {

            bool thisResult = !this.trueCondition;

            bool? knownResult = this.engineRef.GetResult(this.definition, this.context);
            IRuleEvaluator evaluator = this.engineRef.GetEvaluator(this.definition);

            bool doEval = reEvaluate || !knownResult.HasValue;



            if (doEval)
            {
                //#TODO implement metric in evaluation
                try
                {
                    thisResult = await evaluator.EvaluateAsync(
                        this.definition,
                        this.context,
                        engineRef.CurrentProcessId,
                        this.engineRef.State,
                        this.engineRef.GetContainer(this.definition, this.context),
                        this.trueCondition,
                        token);

                    
                }
                catch (Exception ex)
                {
                    throw new EvalException(this.definition.Identifier, "Unable to run evaluator", ex);
                }
                // Cache stores the evaluator results only
                this.engineRef.SetResult(this.definition, this.context, thisResult);
                knownResult = thisResult;


            }

            return (knownResult.Value == this.trueCondition);
        }
    }
}

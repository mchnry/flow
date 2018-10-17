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
            // Cache stores the evaluator results only
            this.lastRunResult = engineRef.GetResult(this.definition.Identifier, this.context);
            bool doEval = reEvaluate || !this.lastRunResult.HasValue;



            if (doEval)
            {
                //#TODO implement metric in evaluation
                try
                {
                    this.lastRunResult = await this.rule.EvaluateAsync(this.definition, this.context, processId, state, validations, this.TrueCondition, token);
                }
                catch (Exception ex)
                {
                    throw new EvalException(this.definition.Identifier, "Unable to run evaluator", ex);
                }
                // Cache stores the evaluator results only
                this.engineRef.SetResult(this.definition.Identifier, this.context, this.lastRunResult);


            }

            return (lastRunResult.Value == this.TrueCondition);
        }
    }
}

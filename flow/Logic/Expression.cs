using Mchnry.Flow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public class Expression : IRule
    {
        private readonly Define.Rule definition;
        private readonly bool trueCondition;
        private readonly Operand condition;
        private readonly IRule first;
        private readonly IRule second;
        private readonly IRuleEngine engineRef;

        internal Expression(Define.Rule definition,
            Operand condition,
            IRule first,
            IRule second,
            IRuleEngine engineRef)
        {
            this.definition = definition;
            this.condition = condition;
            this.first = first;
            this.second = second;
            this.engineRef = engineRef;
        }

        public async Task<bool> EvaluateAsync(bool reEvaluate, IStepTracer<string> tracer, CancellationToken token)
        {
            bool toReturn = true;
            bool testFirst = await first.EvaluateAsync(reEvaluate, tracer, token);
            
            if (this.condition == Operand.And)
            {
                if (!testFirst)
                {
                    toReturn = false;
                } else
                {
                    bool testSecond = await second.EvaluateAsync(reEvaluate, tracer, token);
                    toReturn = testFirst && testSecond;
                }

            } else
            {
                if (testFirst)
                {
                    toReturn = true;
                } else
                {
                    bool testSecond = await second.EvaluateAsync(reEvaluate, tracer, token);
                    toReturn = testSecond;
                }
            }

            return toReturn;
        }
    }
}

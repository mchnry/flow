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
        private readonly Engine engineRef;

        internal Expression(Define.Rule definition,
            Operand condition,
            IRule first,
            IRule second,
            Engine engineRef)
        {
            this.definition = definition;
            this.condition = condition;
            this.first = first;
            this.second = second;
            this.engineRef = engineRef;
        }

        public async Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token)
        {
            bool toReturn = true;
            bool testFirst = await first.EvaluateAsync(reEvaluate, token);

            if (this.condition == Operand.And)
            {
                if (!testFirst)
                {
                    toReturn = false;
                }
                else
                {
                    bool testSecond = await second.EvaluateAsync(reEvaluate, token);
                    toReturn = testFirst && testSecond;
                }

            }
            else
            {
                if (testFirst)
                {
                    toReturn = true;
                }
                else
                {
                    bool testSecond = await second.EvaluateAsync(reEvaluate, token);
                    toReturn = testSecond;
                }
            }

            return toReturn;
        }
    }
}

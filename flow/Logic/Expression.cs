using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    public class Expression<TModel> : IRule<TModel>
    {
        private readonly Define.Rule definition;
        private readonly Operand condition;
        private readonly IRule<TModel> first;
        private readonly IRule<TModel> second;
        private readonly Engine<TModel> engineRef;

        internal Expression(Define.Rule definition,
            Operand condition,
            IRule<TModel> first,
            IRule<TModel> second,
            Engine<TModel> engineRef)
        {
            this.definition = definition;
            this.condition = condition;
            this.first = first;
            this.second = second;
            this.engineRef = engineRef;
        }

        public string Id => this.definition.Id;

        public string RuleIdWithContext => this.definition.RuleIdWithContext;
        public string ShortHand => this.definition.ShortHand;

        public async Task<bool> EvaluateAsync(bool reEvaluate, CancellationToken token)
        {
            bool toReturn = true;

            StepTraceNode<ActivityProcess> mark = this.engineRef.Tracer.CurrentStep = this.engineRef.Tracer.TraceStep(
                new ActivityProcess(this.definition.Id, ActivityStatusOptions.Expression_Evaluating, null));

            bool testFirst = await first.EvaluateAsync(reEvaluate, token);

            if (this.condition == Operand.And)
            {
                if (!testFirst)
                {
                    this.engineRef.Tracer.TraceStep(new ActivityProcess(this.second.Id, ActivityStatusOptions.Rule_NotRun_ShortCircuit, null));

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
                    this.engineRef.Tracer.TraceStep(new ActivityProcess(this.second.Id, ActivityStatusOptions.Rule_NotRun_ShortCircuit, null));
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

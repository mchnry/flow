using System;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow.Logic
{
    internal class DynamicEvaluator<TModel> : IEvaluatorRule<TModel>
    {
        private Func<IEngineScope<TModel>, IEngineTrace, CancellationToken, Task<bool>> evaluator;
        private Evaluator definition;

        public DynamicEvaluator(Evaluator definition, Func<IEngineScope<TModel>, IEngineTrace, CancellationToken, Task<bool>> evaluator)
        {
            this.evaluator = evaluator;
            this.definition = definition;
        }

        public Evaluator Definition => this.definition;

        public async Task<bool> EvaluateAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token)
        {
            return await this.evaluator(scope, trace, token);
        }
    }

    internal class DynamicValidator<TModel> : IValidatorRule<TModel>
    {
        private Func<IEngineScope<TModel>, IEngineTrace, IRuleResult, CancellationToken, Task> validator;
        private Evaluator definition;

        public DynamicValidator(Evaluator definition, Func<IEngineScope<TModel>, IEngineTrace, IRuleResult, CancellationToken, Task> validator)
        {
            this.validator = validator;
            this.definition = definition;
        }

        public Evaluator Definition => this.definition;

        public async Task ValidateAsync(IEngineScope<TModel> scope, IEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            await this.validator(scope, trace, result, token);
        }

     
    }
}

using System;

namespace Mchnry.Flow
{
    /// <summary>
    /// Passed to RuleEvaluator Implementations so that the 
    /// implementation can report its result to the engine.
    /// </summary>
    public interface IRuleResult
    {

        /// <summary>
        /// the result of the evaluation is false.
        /// </summary>
        /// <param name="validation">A validation to return to the caller.</param>
        void Fail(Validation validation);

        /// <summary>
        /// the result of the evaluation is true.
        /// </summary>
        void Pass();

    }

    internal class RuleResult : IRuleResult
    {
        private readonly Action<bool, Validation> status;

        public RuleResult(Action<bool, Validation> status) 
        {
            this.status = status;
        }


        void IRuleResult.Fail(Validation validation)
        {
            status(false, validation);
        }


        void IRuleResult.Pass()
        {
            status(true, null);
        }
    }
}

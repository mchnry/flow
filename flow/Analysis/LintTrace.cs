using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Analysis
{

    public enum LintStatusOptions
    {
        Sanitizing,
        Loading,
        Linting,

        Inspecting,
        LazyDefinition,
        InferringEquation,
        InferringActivity


    }

    public struct LintTrace
    {


        public LintTrace(LintStatusOptions status, string message) : this(status, message, null) { }
        public LintTrace(LintStatusOptions status, string message, string itemId)
        {
            this.Status = status;
            this.Message = message;
            this.ItemId = itemId;
        }

        public LintStatusOptions Status { get; }
        public string Message { get; }
        public string ItemId { get; }

    }
}

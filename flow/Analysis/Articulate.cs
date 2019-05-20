using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Analysis
{

    public interface IArticulateExpression { string Id { get; } }
    public interface IArticulateActivity {  string Id { get; } }

    public class ArticulateAction : IArticulateActivity
    {
        public string Id { get; set; }
        public ArticulateContext Context { get; set; }

    }
    public class NothingAction: IArticulateActivity
    {
        public string Id => "Nothing";
    }

    public class ArticulateActivity : IArticulateActivity
    {
        public string Id { get; set; }
        public List<ArticulateReaction> Reactions { get; set; }
    }

    public class ArticulateContext
    {
        public string Literal { get; set; }
        public string Value { get; set; }
    }

    public class ArticulateReaction
    {
        public IArticulateExpression If { get; set; }
        public IArticulateActivity Then { get; set; }
    }

    public class ArticulateEvaluator : IArticulateExpression
    {
        public string Id { get; set; }
        public bool TrueCondition { get; set; }
        public ArticulateContext Context { get; set; }
    }

    public class ArticulateExpression: IArticulateExpression
    {
        public string Id { get; set; }
        public IArticulateExpression First { get; set; }
        public string Condition { get; set; }
        public IArticulateExpression Second { get; set; }
    }

    public class TrueExpression: IArticulateExpression
    {
        public string Id => "Always";
    }


}

using System.Collections.Generic;

namespace Mchnry.Flow.Analysis
{
    public class LogicTest
    {




        public LogicTest(string equationId, bool isRoot)
        {
            this.IsRoot = isRoot;
            this.EquationId = equationId;
        }

        public bool IsRoot { get; set; }
        public List<Case> TestCases { get; set; }

        public string EquationId { get; internal set; }

    }


}

using System.Collections.Generic;

namespace Mchnry.Flow.Analysis
{
    public class LogicTest
    {




        public LogicTest(string equationId)
        {
            this.EquationId = equationId;
        }

        public List<Case> TestCases { get; set; }

        public string EquationId { get; internal set; }

    }
}

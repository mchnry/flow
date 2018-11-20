using System;
using System.Collections.Generic;
using System.Text;

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

using System;
using System.Collections.Generic;
using System.Text;
using Mchnry.Flow.Logic.Define;
using System.Linq;

namespace Mchnry.Flow.Test
{
    public class Case: ICloneable
    {
        internal Case(List<Rule> rules)
        {
            this.Rules = rules;
        }

        public bool? Result { get; set; } = null;

        public List<Rule> Rules { get; set; } = new List<Rule>();

        public object Clone()
        {

            return new Case((from r in this.Rules select r).ToList());
        }
    }
}

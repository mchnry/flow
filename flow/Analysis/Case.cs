﻿using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mchnry.Flow.Analysis
{
    public class Case : ICloneable
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

using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Configuration
{

    public enum NamePrefixOptions
    {
        Equation = 0,
        Evaluator = 1,
        Activity = 2,
        Action = 3
    };

    public enum ParseOptions { CamelCase, UnderScore }

    public class Convention
    {
        private Dictionary<NamePrefixOptions, string> prefixes = new Dictionary<NamePrefixOptions, string>();
        internal Convention()
        {
            prefixes.Add(NamePrefixOptions.Action, "action");
            prefixes.Add(NamePrefixOptions.Activity, "activity");
            prefixes.Add(NamePrefixOptions.Equation, "equation");
            prefixes.Add(NamePrefixOptions.Evaluator, "evaluator");
        }

        public string GetPrefix(NamePrefixOptions option)
        {
            return prefixes[option];
        }
        public void SetPrefix(NamePrefixOptions option, string value)
        {
            prefixes[option] = value;
        }

        public ParseOptions ParseMethodNamesAs { get; set; } = ParseOptions.CamelCase;

        public string Delimeter { get; set; } = ".";
       


    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Configuration
{
    public class ConventionHelper
    {

        public static string ChangePrefix(NamePrefixOptions from, NamePrefixOptions to, string currentName, Convention convention)
        {

            string toReturn = currentName;
            string toPrefix = convention.GetPrefix(to) + convention.Delimeter;
            string fromPrefix = convention.GetPrefix(from) + convention.Delimeter;

            if (from == NamePrefixOptions.Activity && to == NamePrefixOptions.Action)
            {
                throw new ConventionMisMatchException(currentName, "Cannot change activity to action");
            }
            if (from == NamePrefixOptions.Equation && to == NamePrefixOptions.Evaluator)
            {
                throw new ConventionMisMatchException(currentName, "Cannot change equation to evaluator");
            }
            if ((from == NamePrefixOptions.Evaluator || from == NamePrefixOptions.Equation) && (to == NamePrefixOptions.Activity || to == NamePrefixOptions.Action))
            {
                throw new ConventionMisMatchException(currentName, "Cannot change evaluator to action/activity");
            }
            if ((from == NamePrefixOptions.Action || from == NamePrefixOptions.Activity) && (to == NamePrefixOptions.Equation || to == NamePrefixOptions.Evaluator)) {
                throw new ConventionMisMatchException(currentName, "Cannot change action to evaluator/equation");
            }
            if (currentName.IndexOf(fromPrefix) == -1)
            {
                throw new ConventionMisMatchException(currentName, string.Format("{0} not found in {1}", fromPrefix, currentName));
            } else
            

            toReturn = currentName.Replace(fromPrefix, toPrefix, StringComparison.OrdinalIgnoreCase);
            return toReturn;
        }

        public static string NegateEquationName(string currentName, Convention convention)
        {



            string toReplace = convention.GetPrefix(NamePrefixOptions.Equation) + convention.Delimeter;
            string toReturn = currentName.Replace(toReplace, "", StringComparison.OrdinalIgnoreCase);

            if (currentName.IndexOf(toReplace) == -1)
            {
                throw new ConventionMisMatchException(currentName, string.Format("{0} not found in {1}", toReplace, currentName));
            }

            toReturn = string.Format("{0}{1}{2}{3}", toReplace, "NOT", convention.Delimeter, toReturn);

            return toReturn;
        }

    }
}
using System;
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

            toReturn = currentName.Replace(fromPrefix, toPrefix, StringComparison.OrdinalIgnoreCase);
            return toReturn;
        }

        public static string NegateEquationName(string currentName, Convention convention)
        {
            string toReplace = convention.GetPrefix(NamePrefixOptions.Equation) + convention.Delimeter;
            string toReturn = currentName.Replace(toReplace, "", StringComparison.OrdinalIgnoreCase);
            toReturn = string.Format("{0}{1}{2}{3}", toReplace, "NOT", convention.Delimeter, toReturn);

            return toReturn;
        }

    }
}

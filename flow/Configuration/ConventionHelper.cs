using System;
using System.Text.RegularExpressions;

namespace Mchnry.Flow.Configuration
{

    public struct Parsed { public string Id; public string Literal; }


    internal class ConventionHelper
    {

        internal static string ChangePrefix(NamePrefixOptions from, NamePrefixOptions to, string currentName, Convention convention)
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
            if ((from == NamePrefixOptions.Action) && (to == NamePrefixOptions.Equation || to == NamePrefixOptions.Evaluator))
            {
                throw new ConventionMisMatchException(currentName, "Cannot change action to evaluator/equation");
            }
            if (currentName.IndexOf(fromPrefix) == -1)
            {
                throw new ConventionMisMatchException(currentName, string.Format("{0} not found in {1}", fromPrefix, currentName));
            }
            else
            {

         
               
                toReturn = currentName.Replace(fromPrefix, toPrefix);
            }
            return toReturn;
        }

        internal static string TrueEquation(Convention convention)
        {
            return ApplyConvention(NamePrefixOptions.Equation, "true", convention);
        }
        internal static string TrueEvaluator(Convention convention)
        {
            return ApplyConvention(NamePrefixOptions.Evaluator, "true", convention);
        }

        internal static string ApplyConvention(NamePrefixOptions to, string currentName, Convention convention )
        {
            string toReturn = null;

            string prefix = convention.GetPrefix(to) + convention.Delimeter;
            toReturn = prefix + currentName;

            return toReturn;
        }

        internal static string NegateEquationName(string currentName, Convention convention)
        {



            string toReplace = convention.GetPrefix(NamePrefixOptions.Equation) + convention.Delimeter;
            string toReturn = currentName.Replace(toReplace, "");

            if (currentName.IndexOf(toReplace) == -1)
            {
                throw new ConventionMisMatchException(currentName, string.Format("{0} not found in {1}", toReplace, currentName));
            }

            toReturn = string.Format("{0}{1}{2}{3}", toReplace, "NOT", convention.Delimeter, toReturn);

            return toReturn;
        }


        internal static bool MatchesConvention(NamePrefixOptions conventionToCheck, string Id, Convention convention)
        {
            string toCheck = convention.GetPrefix(conventionToCheck) + convention.Delimeter;
            return Id.StartsWith(toCheck);

        }

        internal static string EnsureConvention(NamePrefixOptions to, string id, Convention convention)
        {
            NamePrefixOptions? match;
            if (MatchesConvention(to, id, convention))
            {
                return id;
            } else if ((match = MatchesAnyConvention(id, convention)) != null)
            {
                throw new ConventionMisMatchException("id", "Already conventionalized");
            } else
            {
                string toCheck = convention.GetPrefix(to) + convention.Delimeter;
                return string.Format("{0}{1}", toCheck, id);
            }
        }

        internal static NamePrefixOptions? MatchesAnyConvention(string id, Convention convention)
        {
            if (MatchesConvention(NamePrefixOptions.Action, id, convention)) return NamePrefixOptions.Action;
            if (MatchesConvention(NamePrefixOptions.Activity, id, convention)) return NamePrefixOptions.Activity;
            if (MatchesConvention(NamePrefixOptions.Equation, id, convention)) return NamePrefixOptions.Equation;
            if (MatchesConvention(NamePrefixOptions.Evaluator, id, convention)) return NamePrefixOptions.Evaluator;
            return null;
        }

        internal static string RemoveConvention(string id, Convention convention)
        {
            NamePrefixOptions? found = MatchesAnyConvention(id, convention);
            string toReturn = id;
            if (found != null)
            {
                string prefix = convention.GetPrefix(found.Value);
                string delimeter = convention.Delimeter;
                string toRemove = string.Format("{0}{1}", prefix, delimeter);
                toReturn = id.Replace(toRemove, "");
            }
            return toReturn;
        }

        public static Parsed ParseMethodName(string toParse, ParseOptions opt)
        {

            Parsed toReturn = new Parsed() { Id = toParse };
            string[] parts = new string[] { };
            if (opt == ParseOptions.UnderScore)
            {
                parts = toParse.Split('_');
            }

            //if not underscore, or no underscors in string, do camelcase
            if (parts.Length == 0 || parts.Length == 1)
            {
                toParse = Regex.Replace(toParse, @"(?<!_)([A-Z])", "_$1");
                parts = toParse.Split('_');
            }

            toReturn.Literal = string.Join(" ", parts).TrimStart(' ');
            return toReturn;
        }
    }
}

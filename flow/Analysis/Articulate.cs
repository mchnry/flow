using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;

namespace Mchnry.Flow.Analysis
{

    public class Friendly
    {
        internal static string tabs(int indent)
        {
            return new string('\t', indent);
        }
        internal static bool IsAlwaysTrue(string id)
        {
            return id == "evaluator.true" || id == new TrueExpression().Id;
        }
    }

    public interface IArticulateExpression {
        public bool TrueCondition { get; set; }
        string Id { get; }
        string Literal { get; }
        string ReaderFriendly(int indent);
    }

    public interface IArticulateActivity {
        string Id { get; }
        string Literal { get; }
        string ReaderFriendly(int indent);

    }

    public class ArticulateAction : IArticulateActivity
    {
        public string Id { get; set; }
        public ArticulateContext Context { get; set; }
        public string Literal { get; set; }
        public string ReaderFriendly(int indent)
        {
            string ctx = Context != null ? "|" + Context.ReaderFriendly() : "";
            string toWrite = $"{Friendly.tabs(indent)}DO->[{this.Literal}{ctx}]";
            return toWrite;
    
        }

    }
    public class NothingAction: IArticulateActivity
    {
        public string Id => "Nothing";
        public string Literal => "Do Nothing";
        public string ReaderFriendly(int indent)
        {
            string toWrite = $"{Friendly.tabs(indent)}DO->[Nothing]";
            return toWrite;

        }
    }

    public class ArticulateActivity : IArticulateActivity
    {
        public string Id { get; set; }
        public List<ArticulateReaction> Reactions { get; set; }
        public string Literal => "Activity";
        public string ReaderFriendly(int indent)
        {
            string toWrite = $"{Friendly.tabs(indent)}<BEGIN {this.Literal} - {this.Id}>";
            if (Reactions != null && Reactions.Count > 0)
            {
                toWrite = toWrite + "\n";

                int cnt = Reactions.Count;
                int i = 0;
                foreach (var reaction in Reactions)
                {
                    i++;
                    string toAdd = reaction.ReaderFriendly(indent + 1);
                    if (i < cnt) toAdd += "\n";
                    toWrite = toWrite + toAdd;
                    
                }
          
            }
            
            return toWrite + $"\n{Friendly.tabs(indent)}<END {this.Literal}>";

        }
    }

    public class ArticulateContext
    {
        public string Literal { get; set; }
        public string Value { get; set; }

        public string ReaderFriendly()
        {
            return $"* {Literal} = {Value} *";
        }
    }

    public class ArticulateReaction
    {
        public IArticulateExpression If { get; set; }
        public IArticulateActivity Then { get; set; }
        public string ReaderFriendly(int indent)
        {
           
            string toWrite = string.Empty;
            string tabs = Friendly.tabs(indent);
            if (Friendly.IsAlwaysTrue(If.Id))
            {

                toWrite = $"{Then.ReaderFriendly(indent)}";
            } else
            {
                toWrite = $"{tabs}IF...\n{If.ReaderFriendly(indent)}\n{tabs}THEN...\n{Then.ReaderFriendly(indent)}";
            }
            
            return toWrite;

        }
    }

    public class ArticulateEvaluator : IArticulateExpression
    {
        public string Id { get; set; }
        public bool TrueCondition { get; set; }
        public ArticulateContext Context { get; set; }
        public string Literal { get; set; }
        public string ReaderFriendly(int indent)
        {
            string ctx = Context != null ? "|" + Context.ReaderFriendly() : "";
            string negate = TrueCondition ? "" : "NOT ";
            string toWrite = $"{Friendly.tabs(indent)}{negate}({this.Literal}{ctx})";
            return toWrite;

        }
    }

    public class ArticulateExpression: IArticulateExpression
    {
        public string Id { get; set; }
        public IArticulateExpression First { get; set; }
        public bool TrueCondition { get; set; }
        public string Condition { get; set; }
        public IArticulateExpression Second { get; set; }
        public string Literal => "Equation";
    
        public string ReaderFriendly(int indent)
        {
            string toReturn = string.Empty;
            string tabs = Friendly.tabs(indent);
            string tabsPlus = Friendly.tabs(indent +1);
         
            
            if (Friendly.IsAlwaysTrue(First.Id) && Friendly.IsAlwaysTrue(Second.Id))
            {
                
                toReturn = new TrueExpression().ReaderFriendly(indent);
            }
            if (Friendly.IsAlwaysTrue(First.Id))
            {
                if (!TrueCondition) toReturn = $"{Friendly.tabs(indent)}NOT (\n";
                toReturn = toReturn + Second.ReaderFriendly(indent + ((TrueCondition) ? 0 : 1));
                if (!TrueCondition) toReturn += $"\n{Friendly.tabs(indent)})";

            } else if (Friendly.IsAlwaysTrue(Second.Id)) {
                if (!TrueCondition) toReturn = $"{Friendly.tabs(indent)}NOT (\n";
                toReturn = toReturn + First.ReaderFriendly(indent + ((TrueCondition) ? 0 : 1));
                if (!TrueCondition) toReturn += $"\n{Friendly.tabs(indent)})";
            } else
            {
                string negate = TrueCondition ? "" : "NOT ";
                toReturn = $"{tabs}{negate}(\n{First.ReaderFriendly(indent + 1)}\n{tabsPlus}{Condition.ToUpper()}\n{Second.ReaderFriendly(indent + 1)}\n{tabs})";

            }
            return toReturn;

        }
    }

    public class TrueExpression: IArticulateExpression
    {
        public string Id => "Always";
        public string Literal => "Always True";
        public bool TrueCondition { get; set; }
        public string ReaderFriendly(int indent)
        {
            string toWrite = $"{Friendly.tabs(indent)}({this.Literal})";
            return toWrite;

        }
    }


}

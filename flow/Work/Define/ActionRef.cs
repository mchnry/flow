using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Work.Define
{
    public struct ActionRef
    {

        public static implicit operator ActionRef(string action)
        {
            ActionRef toReturn = new ActionRef();

            if (action.Contains('|'))
            {
                string[] parts = action.Split('|');
                toReturn.ActionId = parts[0];
                toReturn.ActionId = parts[1];
            }
            else toReturn = action;

            return toReturn;
        }

        public string ActionId { get; set; }
        public string Context { get; set; }

    }
}

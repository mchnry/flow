using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Work.Define
{
    public class ActionRef
    {

        public static implicit operator ActionRef(string action)
        {
            ActionRef toReturn = new ActionRef();

    

            if (action.Contains('|'))
            {
                string[] parts = action.Split('|');
                toReturn.ActionId = parts[0];
                toReturn.Context = parts[1];
            }
            else toReturn = new ActionRef() { ActionId = action };

            return toReturn;
        }

        public string ActionId { get; set; }
        public string Context { get; set; }

    }
}

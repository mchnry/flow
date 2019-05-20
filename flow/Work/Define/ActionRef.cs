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

    

            if (action.Contains("|"))
            {
                string[] parts = action.Split('|');
                toReturn.Id = parts[0];
                toReturn.Context = parts[1];
            }
            else toReturn = new ActionRef() { Id = action };

            return toReturn;
        }

        public string Id { get; set; }
        public Context Context { get; set; }

        public override string ToString()
        {
            if (this.Context != null) {
                return $"{this.Id}|{this.Context}";
            } else {
                return this.Id;
            }
        }

    }
}

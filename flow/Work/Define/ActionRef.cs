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
                toReturn.Input = parts[1];
            }
            else toReturn = new ActionRef() { Id = action };

            return toReturn;
        }

        public string Id { get; set; }
        public string Input { get; set; }

        public override string ToString()
        {
            if (this.Input != null) {
                return $"{this.Id}|{this.Input}";
            } else {
                return this.Id;
            }
        }

    }
}

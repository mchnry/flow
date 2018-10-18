using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Logic.Define
{
    public class Equation
    {

        public string Id { get; set; }
        
        [JsonProperty("f")]
        public Rule First { get; set; }

        [JsonProperty("s")]
        public Rule Second { get; set; }

        [JsonProperty("c")]
        public Operand Condition { get; set; }


    }
}

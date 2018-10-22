using Newtonsoft.Json;

namespace Mchnry.Flow.Logic.Define
{
    public struct Equation
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

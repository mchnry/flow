using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mchnry.Flow.Work.Define
{
    public struct Activity
    {

        public string Id { get; }
        [JsonProperty("X")]
        public ActionRef Action { get; }
        [JsonProperty("RXV")]
        public List<Reaction> Reactions { get; set; }
    }

    public struct Reaction
    {


        [JsonProperty("EQId")]
        public string EquationId { get; set; }
        [JsonProperty("XVId")]
        public string ActivityId { get; set; }
    }
}

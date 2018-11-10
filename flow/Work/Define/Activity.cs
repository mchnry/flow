using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mchnry.Flow.Work.Define
{
    public class Activity
    {

        public string Id { get; set; }
        [JsonProperty("X")]
        public ActionRef Action { get; set; }
        [JsonProperty("RXV")]
        public List<Reaction> Reactions { get; set; }
    }

    public class Reaction
    {


        [JsonProperty("EQId")]
        public string EquationId { get; set; }
        [JsonProperty("XVId")]
        public string ActivityId { get; set; }
    }
}

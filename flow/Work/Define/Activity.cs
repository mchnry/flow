using Newtonsoft.Json;
using System;
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



        [JsonProperty("L")]
        public string Logic { get; set; }
        [JsonProperty("W")]
        public string Work { get; set; }
    }
}

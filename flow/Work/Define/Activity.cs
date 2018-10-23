using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mchnry.Flow.Work.Define
{
    public struct Activity
    {
        public Activity(string id, string actionId)
        {
            this.Id = id;
            this.ActionId = actionId;
            this.Reactions = new List<Reaction>();

        }
        public Activity(string id, string actionId, List<Reaction> reactions) : this(id, actionId)
        {
            this.Reactions = reactions;
        }

        public string Id { get; }
        [JsonProperty("X")]
        public string ActionId { get; }
        [JsonProperty("RXV")]
        public List<Reaction> Reactions { get; set; }
    }

    public struct Reaction
    {


        public Reaction(string equationId, string activityId)
        {
            this.EquationId = equationId;
            this.ActivityId = activityId;
        }

        [JsonProperty("EQId")]
        public string EquationId { get; set; }
        [JsonProperty("XVId")]
        public string ActivityId { get; set; }
    }
}

using Newtonsoft.Json;

namespace Mchnry.Flow.Logic.Define
{
    public class Evaluator
    {
        public string Id { get; set; }

        [JsonProperty("d")]
        public string Description { get; set; }
    }
}

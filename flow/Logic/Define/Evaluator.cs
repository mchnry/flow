using Newtonsoft.Json;

namespace Mchnry.Flow.Logic.Define
{
    public struct Evaluator
    {
        public string Id { get; set; }

        [JsonProperty("d")]
        public string Description { get; set; }
    }
}

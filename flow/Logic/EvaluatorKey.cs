namespace Mchnry.Flow.Logic
{
    public struct EvaluatorKey
    {
        private string context;

        public string Id { get; set; }
        public string Context {
            get {
                return this.context ?? string.Empty;

            }
            set { this.context = value; }
        }

    }
}

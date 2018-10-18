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

        public override string ToString()
        {

            string toReturn = this.Id;
            if (!string.IsNullOrEmpty(this.context))
            {
                toReturn = string.Format("{0}.{1}", this.Id, this.context);
            }
            return toReturn;
        }

    }
}

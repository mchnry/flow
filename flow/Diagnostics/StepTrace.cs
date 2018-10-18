namespace Mchnry.Flow.Diagnostics
{
    using System.Collections.Generic;

    public class StepTrace
    {


        private Dictionary<string, StepTrace> children;
        private string value;
        public static implicit operator StepTrace(string value)
        {
            return new StepTrace(value);
        }
        \
        public StepTrace(string value, List<KeyValuePair<string, StepTrace>> nodes) : this(value)
        {
            if ((nodes != null) && (nodes.Count > 0))
            {
                this.children = new Dictionary<string, StepTrace>();
                nodes.ForEach(g =>
                {
                    this.children.Add(g.Key, g.Value);
                });
            }
        }

        internal StepTrace(string value)
        {
            this.value = value;

        }

        public string Value {
            get {
                return this.value ?? string.Empty;
            }
            set {
                this.value = value;
            }
        }

        public Dictionary<string, StepTrace> Nodes {
            get {
                return this.children;
            }
            set {
                this.children = value;
            }
        }
    }
}

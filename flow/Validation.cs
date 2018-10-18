using System;
using System.Collections.Generic;

namespace Mchnry.Flow
{
    public class Validation
    {


        public Validation(string key, ValidationSeverity severity, string validationMessage)
        {

            this.Key = !string.IsNullOrEmpty(key) ? key : throw new ArgumentNullException("Key");
            this.ValidationMessage = !string.IsNullOrEmpty(validationMessage) ? validationMessage : throw new ArgumentNullException("ValidationMessage");
            this.Severity = severity;
            this.Options = new Dictionary<string, string>();
            this.OverRideInstructions = string.Empty;
            switch (severity)
            {

                case ValidationSeverity.Escalate:
                    this.OverRideInstructions = "Override?"; break;
                case ValidationSeverity.Confirm:
                    this.OverRideInstructions = "Confirm?"; break;
            }

        }

        public Validation(string key, ValidationSeverity severity, string validationMessage, string overRideInstructions)
        {

            this.OverRideInstructions = string.IsNullOrEmpty(overRideInstructions) ? this.OverRideInstructions : overRideInstructions;
        }

        public Validation(string key, ValidationSeverity severity, string validationmessage, string overRideInstructions, Dictionary<string, string> options) :
            this(key, severity, validationmessage, overRideInstructions)
        {
            if (options == null) options = new Dictionary<string, string>();
            this.Options = options;
        }

        public string Key { get; internal set; }
        public ValidationSeverity Severity { get; internal set; }
        public string ValidationMessage { get; internal set; }
        public string OverRideInstructions { get; internal set; }
        public Dictionary<string, string> Options { get; internal set; }


        public ValidationOverride CreateOverride(string reason, string auditCode)
        {

            if (this.Options.Count > 0)
            {
                if (!this.Options.ContainsKey(reason))
                {
                    throw new ArgumentException("Invalid override reason");
                }
            }

            return new ValidationOverride(this.Key, reason, auditCode);
        }

    }
}

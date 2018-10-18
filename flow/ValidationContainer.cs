using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Mchnry.Flow
{
    public class ValidationContainer : IValidationContainer
    {

        [JsonProperty]
        internal List<Validation> Validations { get; set; } = new List<Validation>();
        [JsonProperty]
        internal List<ValidationOverride> Overrides { get; set; } = new List<ValidationOverride>();

        public ValidationContainer() { }
        public ValidationContainer(List<Validation> validations, List<ValidationOverride> overrides) : this()
        {
            if (validations != null) this.Validations = validations;
            if (overrides != null) this.Overrides = overrides;
        }

        public bool ResolveValidations()
        {
            throw new NotImplementedException();
        }

        internal virtual void UpsertValidation(Validation toAdd)
        {

            var current = this.Validations.FirstOrDefault(g => g.Key.Equals(toAdd.Key, StringComparison.OrdinalIgnoreCase));
            if (current != null)
            {
                this.Validations.Remove(current);
            }

            this.Validations.Add(toAdd);

        }

        internal virtual void UpsertOverride(ValidationOverride toAdd)
        {
            var current = this.Overrides.FirstOrDefault(g => g.Key.Equals(toAdd.Key, StringComparison.OrdinalIgnoreCase));
            if (current != null)
            {
                this.Overrides.Remove(current);
            }

            this.Overrides.Add(toAdd);
        }


        ReadOnlyCollection<ValidationOverride> IValidationContainer.Overrides => throw new NotImplementedException();

        ReadOnlyCollection<Validation> IValidationContainer.Validations => throw new NotImplementedException();

        void IValidationContainer.AddOverride(string key, string comment, string auditCode)
        {
            Validation existing = this.Validations.FirstOrDefault(g => g.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            ValidationOverride toAdd = default(ValidationOverride);
            if (existing != null)
            {
                toAdd = existing.CreateOverride(comment, auditCode);
                toAdd.Redeemed = true;
            }
            else
            {
                toAdd = new ValidationOverride(key, comment, auditCode);
            }
            this.UpsertOverride(toAdd);

        }

        void IValidationContainer.AddValidation(Validation toAdd)
        {
            throw new NotImplementedException();
        }
    }
}

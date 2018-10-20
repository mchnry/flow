using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mchnry.Flow
{
    public class ValidationContainer : IValidationContainer
    {

        [JsonProperty]
        internal List<Validation> MyValidations { get; set; } = new List<Validation>();
        [JsonProperty]
        internal List<ValidationOverride> MyOverrides { get; set; } = new List<ValidationOverride>();

        public ValidationContainer() { }

        public bool ResolveValidations()
        {
            List<ValidationOverride> orides;
            List<Validation> vals;

            if (!string.IsNullOrEmpty(this.scopeId))
            {
                orides = this.MyOverrides.Where(g => g.Key.ToLower().StartsWith(this.scopeId.ToLower() + ".")).ToList();
                vals = this.MyValidations.Where(g => g.Key.ToLower().StartsWith(this.scopeId.ToLower() + ".")).ToList();
            }
            else
            {
                orides = this.MyOverrides;
                vals = this.MyValidations;
            }

            bool toReturn = true;
            if (vals.Count() > 0)
            {
                toReturn = false;

                if (orides.Count > 0)
                {
                    toReturn = true;
                    vals.Where(v => v.Severity != ValidationSeverity.Fatal).ToList().ForEach(v =>
                    {
                        toReturn = toReturn && orides.Count(o => o.Key.Equals(v.Key, StringComparison.OrdinalIgnoreCase)) > 0;
                    });
                }

            }

            return toReturn;
        }

        private void UpsertValidation(Validation toAdd)
        {

            var current = this.MyValidations.FirstOrDefault(g => g.Key.Equals(toAdd.Key, StringComparison.OrdinalIgnoreCase));
            if (current != null)
            {
                this.MyValidations.Remove(current);
            }

            this.MyValidations.Add(toAdd);

        }

        private void UpsertOverride(ValidationOverride toAdd)
        {
            var current = this.MyOverrides.FirstOrDefault(g => g.Key.Equals(toAdd.Key, StringComparison.OrdinalIgnoreCase));
            if (current != null)
            {
                this.MyOverrides.Remove(current);
            }

            this.MyOverrides.Add(toAdd);
        }


        public ReadOnlyCollection<ValidationOverride> Overrides => this.MyOverrides.Where(g => this.scopeId == string.Empty || g.Key.StartsWith(this.scopeId + ".")).ToList().AsReadOnly();

        public ReadOnlyCollection<Validation> Validations {
            get {
                return this.MyValidations.Where(g => this.scopeId == string.Empty || g.Key.StartsWith(this.scopeId + ".")).ToList().AsReadOnly();
            }
        }


        public IValidationContainer Scope(string scopeId)
        {
            return new ValidationContainer()
            {
                scopeId = scopeId,
                MyOverrides = this.MyOverrides,
                MyValidations = this.MyValidations
            };
        }

        private string scopeId { get; set; } = string.Empty;
        string IValidationContainer.ScopeId { get => this.scopeId; }

        void IValidationContainer.AddOverride(string key, string comment, string auditCode)
        {
            string keyToAdd = string.Format("{0}.{1}", this.scopeId, key);
            Validation existing = this.MyValidations.FirstOrDefault(g => g.Key.Equals(keyToAdd, StringComparison.OrdinalIgnoreCase));
            ValidationOverride toAdd = default(ValidationOverride);
            if (existing != null)
            {
                toAdd = existing.CreateOverride(comment, auditCode);
                toAdd.Redeemed = true;
            }
            else
            {
                toAdd = new ValidationOverride(keyToAdd, comment, auditCode);
            }
            this.UpsertOverride(toAdd);

        }

        void IValidationContainer.AddValidation(Validation toAdd)
        {
            string keyToAdd = string.Format("{0}.{1}", this.scopeId, toAdd.Key);

            Validation existing = this.MyValidations.FirstOrDefault(g => g.Key.Equals(keyToAdd, StringComparison.OrdinalIgnoreCase));


            toAdd.Key = keyToAdd;
            this.UpsertValidation(toAdd);

            if (existing == null)
            {
                ValidationOverride existingOverride = this.MyOverrides.FirstOrDefault(g => g.Key.Equals(keyToAdd, StringComparison.OrdinalIgnoreCase));
                if (existingOverride != null)
                {
                    existingOverride.Redeemed = true;

                }

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Mchnry.Flow
{
    public interface IValidationContainer
    {
        ReadOnlyCollection<ValidationOverride> Overrides { get; }
        ReadOnlyCollection<Validation> Validations { get; }

        void AddOverride(string key, string comment, string auditCode);
        void AddValidation(Validation toAdd);
        
        bool ResolveValidations();
    }
}

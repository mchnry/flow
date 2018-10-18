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

        void AddOverride(ValidationOverride toAdd);
        void AddValidation(Validation toAdd);
        void RedeemValidation(ValidationOverride toRedeem);
        bool ResolveValidations();
    }
}

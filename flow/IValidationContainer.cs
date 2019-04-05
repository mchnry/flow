using System.Collections.ObjectModel;

namespace Mchnry.Flow
{
    public interface IValidationContainer
    {
        string ScopeId { get; }
        void AddOverride(string key, string comment, string auditCode);
        void AddValidation(Validation toAdd);

        ReadOnlyCollection<ValidationOverride> Overrides { get; }
        ReadOnlyCollection<Validation> Validations { get; }

        bool ResolveValidations();
    }

}

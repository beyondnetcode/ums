using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.Rules.Impl;

namespace Ums.Shell.Ddd.ValueObjects.Validators
{
    public class IdGuidValidator : AbstractRuleValidator<ValueObject<string>>
    {
        public IdGuidValidator(ValueObject<string> subject) : base(subject)
        {
        }

        public override void AddRules(RuleContext? context)
        {
            var id = Subject.GetValue();

            if (string.IsNullOrEmpty(id))
            {
                AddBrokenRule("IdValueObject", "Id is required");
                return;
            }

            var guid = Guid.Empty;
            var isGuid = Guid.TryParse(id, out guid);

            if (!isGuid)
            {
                AddBrokenRule("IdValueObject", "Id is not a valid Guid");
                return;
            }
        }
    }
}

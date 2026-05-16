using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.Rules.Impl;

namespace Ums.Shell.Ddd.ValueObjects.Validators
{
    /// <summary>
    /// Validates that a string value object meets the minimum length requirement.
    /// </summary>
    public class StringMinLenghtValidator : AbstractRuleValidator<ValueObject<string>>
    {
        private readonly int _minLength;

        public StringMinLenghtValidator(ValueObject<string> subject, int minLength) : base(subject)
        {
            _minLength = minLength;
        }

        public override void AddRules(RuleContext? context)
        {
            var value = Subject.GetValue();

            if (value != null && value.Length < _minLength)
            {
                AddBrokenRule(this.GetType().Name, $"Value for property must be at least {_minLength} characters long");
            }
        }
    }

}

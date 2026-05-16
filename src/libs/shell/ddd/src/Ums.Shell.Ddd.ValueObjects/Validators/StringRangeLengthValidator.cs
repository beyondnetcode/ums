using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.Rules.Impl;

namespace Ums.Shell.Ddd.ValueObjects.Validators
{
    public class StringRangeLenghtValidator : AbstractRuleValidator<ValueObject<string>>
    {
        private readonly int _minLength;
        private readonly int _maxLength;

        public StringRangeLenghtValidator(ValueObject<string> subject, int minLength, int maxLength) : base(subject)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }
        public override void AddRules(RuleContext? context)
        {
            var value = Subject.GetValue();

            if (value != null)
            {
                if (value.Length < _minLength)
                {
                    AddBrokenRule(this.GetType().Name, $"Value for property must be at least {_minLength} characters long");
                }
                if (value.Length > _maxLength)
                {
                    AddBrokenRule(this.GetType().Name, $"Value for property exceeds maximum length of {_maxLength} characters");
                }
            }
        }
    }
}

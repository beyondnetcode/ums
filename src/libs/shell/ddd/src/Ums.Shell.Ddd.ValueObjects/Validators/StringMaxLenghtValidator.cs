using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.Rules.Impl;

namespace Ums.Shell.Ddd.ValueObjects.Validators
{
    public  class StringMaxLenghtValidator : AbstractRuleValidator<ValueObject<string>>
    {
        private readonly int _maxLength;
        
        public StringMaxLenghtValidator(ValueObject<string> subject, int maxLength) : base(subject)
        {
            _maxLength = maxLength;
        }

        public override void AddRules(RuleContext? context)
        {
            var value = Subject.GetValue();

            if (value != null && value.Length > _maxLength)
            {
                AddBrokenRule(this.GetType().Name, $"Value for property exceeds maximum length of {_maxLength} characters");
            }
        }
    }
}

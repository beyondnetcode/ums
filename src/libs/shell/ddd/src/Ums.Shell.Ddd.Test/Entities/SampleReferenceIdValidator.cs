using Ums.Shell.Ddd.Interfaces;

namespace Ums.Shell.Ddd.Test.Entities
{
    public class SampleReferenceIdValidator : AbstractRuleValidator<ValueObject<SampleReferenceIdProps>>
    {
        public SampleReferenceIdValidator(ValueObject<SampleReferenceIdProps> subject) : base(subject)
        {
        }

        public override void AddRules(RuleContext? context)
        {
            var name = Subject.GetValue().Name;

            if (name.Contains("Default"))
            {
                AddBrokenRule("SampleReferenceId", "The name cannot contain the word 'Default'");
                return;
            }

            if (name.Contains("Test"))
            {
                AddBrokenRule("SampleReferenceId", "The name cannot contain the word 'Test'");
                return;
            }

            if (name.Contains("Sample"))
            {
                AddBrokenRule("SampleReferenceId", "The name cannot contain the word 'Sample'");
                return;
            }
        }
    }
}

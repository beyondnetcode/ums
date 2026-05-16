namespace Ums.Shell.Ddd.Test.Entities
{
    public class SampleNameValidator : AbstractRuleValidator<ValueObject<string>>
    {
        public SampleNameValidator(ValueObject<string> subject) : base(subject)
        {
        }

        public override void AddRules(RuleContext? context)
        {
            var value = Subject.GetValue();

            if (string.IsNullOrWhiteSpace(value))
            {
                AddBrokenRule("Value", "The value cannot be null or empty.");
            }


            int maxLength = 0;

            if (context != null)
            {
                var existsLengthParamNullable = context.Parameters.FirstOrDefault(p => p.Item1.ToLower().Trim() == "maxlength");

                if (!existsLengthParamNullable.Equals(default))
                {
                    if (existsLengthParamNullable.Item2 is int max)
                    {
                        maxLength = max;
                    }
                    else if (int.TryParse(existsLengthParamNullable.Item2?.ToString(), out int parsed))
                    {
                        maxLength = parsed;
                    }
                }
            }

            if (maxLength > 0)
            {
                if (value.Length > maxLength)
                {
                    AddBrokenRule("Name", $"Name cannot exceed {maxLength} characters.");

                    //Subject.BrokenRules.Add(new BrokenRule("Name", $"Name cannot exceed {maxLength} characters."));
                }
            }

        }
    }
}
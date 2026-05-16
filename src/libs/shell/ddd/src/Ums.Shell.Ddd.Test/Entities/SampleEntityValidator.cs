namespace Ums.Shell.Ddd.Test.Entities
{
    public class SampleEntityValidator : AbstractRuleValidator<SampleEntity>
    {
        public SampleEntityValidator(SampleEntity subject) : base(subject)
        {
        }

        public override void AddRules(RuleContext? context)
        {
            var props = Subject.Props;

            if (props.Name.GetValue().ToLowerInvariant() == "default")
            {
                AddBrokenRule(nameof(props.Name), "Name cannot be 'default'.");
            }

        }

        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsMorning(DateTime date)
        {
            // Check if the hour is between 0 (midnight) and 11 (11:59 AM)
            return date.Hour >= 0 && date.Hour < 12;
        }

    }
}

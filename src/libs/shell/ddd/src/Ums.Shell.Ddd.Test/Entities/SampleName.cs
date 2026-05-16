using Ums.Shell.Ddd.ValueObjects.Common;

namespace Ums.Shell.Ddd.Test.Entities
{ 
    public class SampleName : StringValueObject
    {
        private SampleName(string value) : base(value)
        {

        }

        public static SampleName Create(string value)
        {
            return new SampleName(value);
        }
        public static SampleName Default()
        {
            return new SampleName(string.Empty);
        }

        public override void AddValidators()
        {
            base.AddValidators();

            ValidatorRules.Add(new SampleNameValidator(this));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GetValue();
        }
    }
}

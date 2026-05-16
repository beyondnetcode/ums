namespace Ums.Shell.Ddd.ValueObjects.Common
{
    public class DecimalValueObject: ValueObject<decimal>
    {
        protected DecimalValueObject(decimal value) : base(value)
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GetValue();
        }

        public static implicit operator DecimalValueObject(decimal value)
        {
            return new DecimalValueObject(value);
        }
    }
}

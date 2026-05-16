namespace Ums.Shell.Ddd.ValueObjects.Common
{
    public class IntValueObject : ValueObject<int>
    {
        protected IntValueObject(int value) : base(value)
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GetValue();
        }

        public static implicit operator IntValueObject(int value)
        {
            return new IntValueObject(value);
        }
    }
}

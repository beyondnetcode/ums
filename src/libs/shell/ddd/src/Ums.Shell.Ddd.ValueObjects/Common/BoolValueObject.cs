namespace Ums.Shell.Ddd.ValueObjects.Common
{
    public class BoolValueObject : ValueObject<bool>
    {
        protected BoolValueObject(bool value) : base(value)
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GetValue();
        }  
        
        public static implicit operator BoolValueObject(bool value)
        {
            return new BoolValueObject(value);
        }
    }
}

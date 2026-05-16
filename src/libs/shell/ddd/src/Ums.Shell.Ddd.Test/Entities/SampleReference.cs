namespace Ums.Shell.Ddd.Test.Entities
{
    public class SampleReferenceProps : IProps
    {
        public IdValueObject Id { get; set; } = default!;
        public SampleName Name { get; set; } = default!;

        public SampleReferenceProps(IdValueObject id, SampleName name)
        {
            Id = id;
            Name = name;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class SampleReference : Entity<SampleReference, SampleReferenceProps>
    {

        private SampleReference(SampleReferenceProps props) : base(props)
        {
        }

        public static SampleReference Create(SampleReferenceProps props)
        {
            return new SampleReference(props);
        }
    }
}

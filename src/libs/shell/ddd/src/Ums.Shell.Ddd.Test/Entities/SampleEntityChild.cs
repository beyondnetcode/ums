namespace Ums.Shell.Ddd.Test.Entities
{
    public class SampleEntityChildProps : IProps
    {
        public IdValueObject Id { get; init; } = default!;
        public IdValueObject ParentId { get; init; } = default!;
        public SampleName Name { get; init; } = default!;
        public SampleEntityChildStatus Status { get; init; } = default!;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class SampleEntityChild : Entity<SampleEntityChild, SampleEntityChildProps>
    {
        public SampleEntityChild(SampleEntityChildProps props) : base(props)
        {
        }
    }

    public class SampleEntityChildStatus : DomainEnumeration
    {
        public static SampleEntityChildStatus Active = new SampleEntityChildStatus(1, nameof(Active));
        public static SampleEntityChildStatus Inactive = new SampleEntityChildStatus(2, nameof(Inactive));
        public SampleEntityChildStatus(int id, string name) : base(id, name)
        {
        }
    }
}

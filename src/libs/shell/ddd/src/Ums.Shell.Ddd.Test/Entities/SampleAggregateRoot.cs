namespace Ums.Shell.Ddd.Test.Entities
{
    public class SampleAggregateRootProps : IProps
    {
        public IdValueObject Id { get; private set; } = default!;
        public SampleName Name { get; private set; } = default!;
        public SampleEntity EntityRef1 { get; private set; } = default!;
        public SampleEntity EntityRef2 { get; private set; } = default!;
        public AuditValueObject Audit { get; private set; } = default!;
        public SampleAggregateRootStatus Status { get; set; } = default!;

        public SampleAggregateRootProps(IdValueObject id,
                                        SampleName name,
                                        SampleEntity entityRef1,
                                        SampleEntity entityRef2)
        {
            Id = id;
            Name = name;
            EntityRef1 = entityRef1;
            EntityRef2 = entityRef2;
            Status = SampleAggregateRootStatus.Active;
            Audit = AuditValueObject.Create("default");
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class SampleAggregateRoot : AggregateRoot<SampleAggregateRoot, SampleAggregateRootProps>
    {
        private SampleAggregateRoot(SampleAggregateRootProps props) : base(props)
        {
            if (TrackingState.IsNew)
            {
                DomainEvents.ApplyChange(new SampleCreatedDomainEvent(props.Id.GetValue(), props.Name.GetValue()), true);
            }
        }

        public static SampleAggregateRoot Create(IdValueObject id, SampleName name, SampleEntity entityRef1, SampleEntity entityRef2)
        {
            var props = new SampleAggregateRootProps(id, name, entityRef1, entityRef2);


            return new SampleAggregateRoot(props);
        }

        public void ChangeName(SampleName name)
        {
            Props.Name.SetValue(name.GetValue());
            Props.Audit.Update("default");
        }

        public void ChangeEntityRef1(SampleEntity entityRef1)
        {
            Props.EntityRef1.SetProps(entityRef1.GetPropsCopy());
            Props.Audit.Update("default");
        }

        public void ChangeEntityRef2(SampleEntity entityRef2)
        {
            Props.EntityRef2.SetProps(entityRef2.GetPropsCopy());
            Props.Audit.Update("default");
        }

        public void Inactivate()
        {
            if (GetPropsCopy().Status == SampleAggregateRootStatus.Inactive)
            {
                throw new InvalidOperationException("SampleAggregateRoot is already inactive.");
            }

            Props.Status = SampleAggregateRootStatus.Inactive;
            Props.Audit.Update("default");
        }

        public void Activate()
        {
            if (GetPropsCopy().Status == SampleAggregateRootStatus.Active)
            {
                throw new InvalidOperationException("SampleAggregateRoot is already active.");
            }

            Props.Status = SampleAggregateRootStatus.Active;
            Props.Audit.Update("default");
        }
    }

    public class SampleAggregateRootStatus: DomainEnumeration
    {
        public static SampleAggregateRootStatus Active = new SampleAggregateRootStatus(1, nameof(Active).ToLowerInvariant());
        public static SampleAggregateRootStatus Inactive = new SampleAggregateRootStatus(2, nameof(Inactive).ToLowerInvariant());

        public SampleAggregateRootStatus(int id, string name) : base(id, name)
        {
        }
    }
}

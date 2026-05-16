namespace Ums.Shell.Ddd.Test.Entities
{
    public class SampleEntityProps: IProps
    {
        public SampleName Name { get; private set; }
        public SampleReferenceId SampleReferenceId { get; set; }
        public SampleEntityStatus Status { get; set; }
        public AuditValueObject Audit { get; private set; }


        public SampleEntityProps(SampleName name, SampleReferenceId sampleReferenceId)
        {
            Name = name;
            SampleReferenceId = sampleReferenceId;
            Status = SampleEntityStatus.Active;
            Audit = AuditValueObject.Create("default");
        }

        public SampleEntityProps(SampleName name, SampleReferenceId sampleReferenceId, SampleEntityStatus status, AuditValueObject audit)
        {
            SampleReferenceId = sampleReferenceId;
            Name = name;
            Status = status;
            Audit = audit;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class SampleEntity : Entity<SampleEntity, SampleEntityProps>
    {
        private SampleEntity(SampleEntityProps props) : base(props)
        {
        }

        public static SampleEntity Create(SampleName name, SampleReferenceId sampleReferenceId)
        {
            var props = new SampleEntityProps(name, sampleReferenceId);

            return new SampleEntity(props);
        }

        public static SampleEntity Load(string id, SampleEntityProps props)
        {
            var sampleEntity = new SampleEntity(props);
            sampleEntity.SetId(id);

            return sampleEntity;

        }

        public void ChangeSampleReference(SampleReferenceId sampleReferenceId)
        {
            Props.SampleReferenceId = sampleReferenceId;
            Props.Audit.Update("default");
        }

        public void ChangeName(StringValueObject name)
        {
            Props.Name.SetValue(name.GetValue());
            Props.Audit.Update("default");
        }

        public void Inactivate()
        {
            if (GetPropsCopy().Status == SampleEntityStatus.Inactive)
            {
                BrokenRules.Add(new BrokenRule("Status", "The entity is already inactive"));
                return;
            }

            Props.Status = SampleEntityStatus.Inactive;
            Props.Audit.Update("default");
        }

        public void Activate()
        {
            if (GetPropsCopy().Status == SampleEntityStatus.Active)
            {
                BrokenRules.Add(new BrokenRule("Status", "The entity is already active"));
                return;
            }

            Props.Status = SampleEntityStatus.Active;
            Props.Audit.Update("default");
        }

        public override void AddValidators()
        {
            ValidatorRules.Add(new SampleEntityValidator(this));
        }
    }

    public class SampleEntityStatus : DomainEnumeration
    {
        public static SampleEntityStatus Active = new SampleEntityStatus(1, "Active");
        public static SampleEntityStatus Inactive = new SampleEntityStatus(2, "Inactive");

        public SampleEntityStatus(int id, string name) : base(id, name)
        {
        }
    }
}

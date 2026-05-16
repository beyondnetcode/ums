namespace Ums.Shell.Ddd.Test
{
    [TestClass]
    public class SampleEntityTest
    {
        SampleEntity sampleEntity = null;

        [TestInitialize]
        public void Setup()
        {
            sampleEntity = SampleEntity.Create(SampleName.Create("foo"), SampleReferenceId.Create(Guid.NewGuid().ToString(),"default"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            sampleEntity = null;
        }

        [TestMethod]
        public void Entity_Should_not_be_equal()
        {
            var id = Guid.NewGuid().ToString();                     

            var other = SampleEntity.Create(SampleName.Create("foo"), SampleReferenceId.Create(Guid.NewGuid().ToString(), "default"));

            sampleEntity.Equals(other).ShouldBeFalse();
        }


        [TestMethod]
        public void Should_Implement_Entity()
        {
            Assert.IsInstanceOfType(sampleEntity, typeof(SampleEntity));
        }

        [TestMethod]
        public void Should_Validate_Entity_NotValid()
        {
            sampleEntity.ChangeName(SampleName.Create(""));
            sampleEntity.IsValid().ShouldBeFalse();
        }

        [TestMethod]
        public void Should_Validate_Entity_Track_IsNew()
        {
            sampleEntity.TrackingState.IsNew.ShouldBeTrue();
        }
    }
}

namespace Ums.Shell.Ddd.Test
{
    [TestClass]
    public class ValueObjectTest
    {   

        [TestMethod]
        public void Should_Implement_ValueObject()
        {
            var fieldName = SampleName.Create("foo");

            Assert.IsInstanceOfType(fieldName, typeof(ValueObject<string>));
        }

        [TestMethod]
        public void Should_Be_Equals()
        {
            var id = Guid.NewGuid().ToString(); 

            var Id1 = IdValueObject.Create();
            var Id2 = IdValueObject.Create();

            Id1.SetValue(IdValueObject.Load(id).GetValue());
            Id2.SetValue(IdValueObject.Load(id).GetValue());

            Id1.Equals(Id2).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Be_not_Equals()
        {
            var Id1 = IdValueObject.Create();
            var Id2 = IdValueObject.Create();

            Id1.Equals(Id2).ShouldBeFalse();
        }

        [TestMethod]
        public void Should_Have_Same_HashCode()
        {
            var fieldName1 = SampleName.Create("foo");
            var fieldName2 = SampleName.Create("foo");

            Assert.AreEqual(fieldName1.GetHashCode(), fieldName2.GetHashCode());
        }

        [TestMethod]
        public void Should_Not_Have_Same_HashCode()
        {
            var fieldName1 = SampleName.Create("foo");
            var fieldName2 = SampleName.Create("bar");

            Assert.AreNotEqual(fieldName1.GetHashCode(), fieldName2.GetHashCode());
        }


        [TestMethod]
        public void Should_Not_Be_Equal_With_Operator()
        {
            var fieldName1 = SampleName.Create("foo");
            var fieldName2 = SampleName.Create("bar");

            Assert.IsTrue(fieldName1 != fieldName2);
        }

        [TestMethod]
        public void Should_Be_Equal_With_Null()
        {
            var fieldName1 = SampleName.Create("foo");
            SampleName? fieldName2 = null;

            Assert.IsFalse(fieldName1 == fieldName2);
        }

        [TestMethod]
        public void Should_Not_Be_Equal_With_Null()
        {
            var fieldName1 = SampleName.Create("foo");
            SampleName? fieldName2 = null;

            Assert.IsTrue(fieldName1 != fieldName2);
        }

        [TestMethod]
        public void Should_Be_Equal_With_Null_With_Operator()
        {
            var fieldName1 = SampleName.Create("foo");
            SampleName? fieldName2 = null;

            Assert.IsFalse(fieldName1.Equals(fieldName2));
        }

        [TestMethod]
        public void Should_Has_BrokenRules()
        {
            var vo = SampleName.Create(" ");

            Assert.IsFalse(vo.IsValid);
        }

        [TestMethod]
        public void Should_Has_BrokenRules_In_Create_Mode()
        {
            var vo = SampleName.Create("");

            Assert.IsFalse(vo.IsValid);
        }

        [TestMethod]
        public void Should_Has_BrokenRules_In_Create_Mode_With_Validation()
        {
            var vo = SampleName.Create("");

            Assert.IsFalse(vo.IsValid);
        }

        [TestMethod]
        public void Should_Has_BrokenRules_In_Create_Mode_With_Validation_With_Validator()
        {
            var vo = SampleName.Create("");

            Assert.IsFalse(vo.IsValid);
        }
     }
}

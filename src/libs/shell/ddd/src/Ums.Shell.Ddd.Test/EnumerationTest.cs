namespace Ums.Shell.Ddd.Test
{
    [TestClass]
    public class EnumerationTest
    {
        [TestMethod]
        public void GetAll_ShouldReturnAllValues()
        {
            var values = DomainEnumeration.GetAll<SampleEntityStatus>().ToList();
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(SampleEntityStatus.Active, values[0]);
            Assert.AreEqual(SampleEntityStatus.Inactive, values[1]);
        }

        [TestMethod]
        public void FromValue_ShouldReturnCorrectValue()
        {
            var value = DomainEnumeration.FromValue<SampleEntityStatus>(1);
            Assert.AreEqual(SampleEntityStatus.Active, value);
        }

        [TestMethod]
        public void FromDisplayName_ShouldReturnCorrectValue()
        {
            var value = DomainEnumeration.FromDisplayName<SampleEntityStatus>("Active");
            Assert.AreEqual(SampleEntityStatus.Active, value);
        }

        [TestMethod]
        public void AbsoluteDifference_ShouldReturnCorrectValue()
        {
            var value = DomainEnumeration.AbsoluteDifference(SampleEntityStatus.Active, SampleEntityStatus.Inactive);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void Equals_ShouldReturnTrue()
        {
            var value = SampleEntityStatus.Active.Equals(SampleEntityStatus.Active);
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse()
        {
            var value = SampleEntityStatus.Active.Equals(SampleEntityStatus.Inactive);
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void GetHashCode_ShouldReturnCorrectValue()
        {
            var value = SampleEntityStatus.Active.GetHashCode();
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void ToString_ShouldReturnCorrectValue()
        {
            var value = SampleEntityStatus.Active.ToString();
            Assert.AreEqual("Active", value);
        }

        [TestMethod]
        public void CompareTo_ShouldReturnCorrectValue()
        {
            var value = SampleEntityStatus.Active.CompareTo(SampleEntityStatus.Inactive);
            Assert.AreEqual(-1, value);
        }

        [TestMethod]
        public void CompareTo_ShouldReturnZero()
        {
            var value = SampleEntityStatus.Active.CompareTo(SampleEntityStatus.Active);
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void CompareTo_ShouldReturnOne()
        {
            var value = SampleEntityStatus.Inactive.CompareTo(SampleEntityStatus.Active);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void Parse_ShouldThrowExceptionWithCorrectMessage()
        {
            try
            {
                DomainEnumeration.FromValue<SampleEntityStatus>(3);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("'3' is not a valid value in Ums.Shell.Ddd.Test.Entities.SampleEntityStatus", ex.Message);
            }
        }

        [TestMethod]
        public void Parse_ShouldThrowExceptionWithCorrectMessage2()
        {
            try
            {
                DomainEnumeration.FromDisplayName<SampleEntityStatus>("Third");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("'Third' is not a valid display name in Ums.Shell.Ddd.Test.Entities.SampleEntityStatus", ex.Message);
            }
        }

    }
}

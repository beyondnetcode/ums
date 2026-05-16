namespace Ums.Shell.Ddd.Test
{
    [TestClass]
    public class SampleNameValidatorTest
    {
        [TestMethod]
        public void Name_Should_Be_Invalid_When_Exceeds_MaxLength()
        {
            var valueProperty = new string('a', 101);
            var nameProperty = SampleName.Create(valueProperty);
            var validator = new SampleNameValidator(nameProperty);

            var ruleContext = new RuleContext();
            ruleContext.Parameters.Add(("MaxLength", 100));

            validator.AddRules(ruleContext);

            validator.Subject.BrokenRules.GetBrokenRules().ShouldAllBe(r => r.Property == "Name" &&
                                            r.Message == "Name cannot exceed 100 characters");
        }

        [TestMethod]
        public void Name_Should_Be_Valid_When_Within_MaxLength()
        {
            var valueProperty = new string('b', 50);
            var nameProperty = SampleName.Create(valueProperty);
            var validator = new SampleNameValidator(nameProperty);

            var ruleContext = new RuleContext();
            ruleContext.Parameters.Add(("MaxLength", 100));

            validator.AddRules(ruleContext);

            Assert.AreEqual(0, validator.Subject.BrokenRules.GetBrokenRules().Count);
        }      
    }
}

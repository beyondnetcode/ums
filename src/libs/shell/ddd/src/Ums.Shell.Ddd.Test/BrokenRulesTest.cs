using Ums.Shell.Ddd.Services.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ums.Shell.Ddd.Test
{
    [TestClass]
    public class BrokenRulesTest
    {
        [TestMethod]
        public void Add_AddsBrokenRuleToList()
        {
            // Arrange
            var brokenRules = new BrokenRulesManager();
            var brokenRule = new BrokenRule("Property", "Message");

            // Act
            brokenRules.Add(brokenRule);

            // Assert
            var result = brokenRules.GetBrokenRules();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(brokenRule, result.First());
        }

        [TestMethod]
        public void Add_DoesNotAddDuplicateBrokenRule()
        {
            // Arrange
            var brokenRules = new BrokenRulesManager();
            var brokenRule = new BrokenRule("Property", "Message");

            // Act
            brokenRules.Add(brokenRule);
            brokenRules.Add(brokenRule);

            // Assert
            var result = brokenRules.GetBrokenRules();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(brokenRule, result.First());
        }

        [TestMethod]
        public void Add_AddsMultipleBrokenRulesToList()
        {
            // Arrange
            var brokenRules = new BrokenRulesManager();
            var brokenRule1 = new BrokenRule("Property1", "Message1");
            var brokenRule2 = new BrokenRule("Property2", "Message2");

            // Act
            brokenRules.Add(brokenRule1);
            brokenRules.Add(brokenRule2);

            // Assert
            var result = brokenRules.GetBrokenRules();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(brokenRule1));
            Assert.IsTrue(result.Contains(brokenRule2));
        }

        [TestMethod]
        public void Remove_RemovesBrokenRuleFromList()
        {
            // Arrange
            var brokenRules = new BrokenRulesManager();
            var brokenRule = new BrokenRule("Property", "Message");
            brokenRules.Add(brokenRule);

            // Act
            brokenRules.Remove(brokenRule);

            // Assert
            var result = brokenRules.GetBrokenRules();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Clear_RemovesAllBrokenRulesFromList()
        {
            // Arrange
            var brokenRules = new BrokenRulesManager();
            var brokenRule1 = new BrokenRule("Property1", "Message1");
            var brokenRule2 = new BrokenRule("Property2", "Message2");
            brokenRules.Add(brokenRule1);
            brokenRules.Add(brokenRule2);

            // Act
            brokenRules.Clear();

            // Assert
            var result = brokenRules.GetBrokenRules();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Entity_Should_Has_BrokenRules()
        {
            var sampleEntity = SampleEntity.Create(SampleName.Create("Default"), SampleReferenceId.Create(Guid.NewGuid().ToString(),"XXX"));

            sampleEntity.IsValid().ShouldBeFalse();
        }

        [TestMethod]
        public void Entity_Should_Has_BrokenRules_In_Properties()
        {
            var sampleEntity = SampleEntity.Create(SampleName.Create("foo"), SampleReferenceId.Create(Guid.NewGuid().ToString(), "XXX"));

            sampleEntity.IsValid().ShouldBeFalse();
        }

        [TestMethod]
        public void Entity_Should_Not_Have_BrokenRules()
        {
            var sampleEntity = SampleEntity.Create(SampleName.Create("BeyondNet"), SampleReferenceId.Create(Guid.NewGuid().ToString(), "XXX"));

            sampleEntity.IsValid().ShouldBeTrue();
        }
    }
}

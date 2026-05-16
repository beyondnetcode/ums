namespace Ums.Shell.Ddd.Test
{
    [TestClass]
    public class NotifyPropertyChangedBaseTests
    {
        private class TestNotifyPropertyChangedBase : AbstractNotifyPropertyChanged
        {
            public TestNotifyPropertyChangedBase()
            {
                // Register properties for testing
                RegisterProperty(nameof(Value1), typeof(int), 0);
                RegisterProperty(nameof(Value2), typeof(string), string.Empty);
            }

            public int Value1
            {
                get => (int)GetValue();
                set => SetValue(value);
            }

            public string? Value2
            {
                get => (string?)GetValue();
                set => SetValue(value);
            }
        }

        [TestMethod]
        public void SetValue_Should_UpdatePropertyValue()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const int expectedValue = 42;

            // Act
            target.Value1 = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, target.Value1);
        }

        [TestMethod]
        public void SetValue_Should_InvokePropertyChangedEvent()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value1);
            bool eventRaised = false;

            target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    eventRaised = true;
                }
            };

            // Act
            target.Value1 = 42;

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void SetValue_Should_NotInvokePropertyChangedEvent_WhenValueIsSame()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value1);
            bool eventRaised = false;

            target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    eventRaised = true;
                }
            };

            // Set initial value
            target.Value1 = 42;
            eventRaised = false;

            // Act
            target.Value1 = 42;

            // Assert
            Assert.IsFalse(eventRaised);
        }

        [TestMethod]
        public void SetValue_Should_InvokePropertyChangedCallback()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value1);
            bool callbackInvoked = false;

            target.RegisterPropertyChangedCallback(propertyName, (sender, args) =>
            {
                callbackInvoked = true;
            });

            // Act
            target.Value1 = 42;

            // Assert
            Assert.IsTrue(callbackInvoked);
        }

        [TestMethod]
        public void SetValue_Should_NotInvokePropertyChangedCallback_WhenValueIsSame()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value1);
            bool callbackInvoked = false;

            target.RegisterPropertyChangedCallback(propertyName, (sender, args) =>
            {
                callbackInvoked = true;
            });

            // Set initial value
            target.Value1 = 42;
            callbackInvoked = false;

            // Act
            target.Value1 = 42;

            // Assert
            Assert.IsFalse(callbackInvoked);
        }

        [TestMethod]
        public void SetValue_Should_InvokePropertyChangedEvent_WhenForceSetValueIsTrue()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value1);
            bool eventRaised = false;

            target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    eventRaised = true;
                }
            };

            // Set initial value
            target.Value1 = 42;
            eventRaised = false;

            // Act
            target.ForceSetValue(42, propertyName);

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void SetValue_Should_InvokePropertyChangedCallback_WhenForceSetValueIsTrue()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value1);
            bool callbackInvoked = false;

            target.RegisterPropertyChangedCallback(propertyName, (sender, args) =>
            {
                callbackInvoked = true;
            });

            // Set initial value
            target.Value1 = 42;
            callbackInvoked = false;

            // Act
            target.ForceSetValue(42, propertyName);

            // Assert
            Assert.IsTrue(callbackInvoked);
        }

        [TestMethod]
        public void SetValue_Should_InvokePropertyChangedEvent_WhenValueIsNull()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value2);
            bool eventRaised = false;

            target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    eventRaised = true;
                }
            };

            // Set initial value
            target.Value2 = "Initial Value";
            eventRaised = false;

            // Act
            target.Value2 = null;

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void SetValue_Should_InvokePropertyChangedCallback_WhenValueIsNull()
        {
            // Arrange
            var target = new TestNotifyPropertyChangedBase();
            const string propertyName = nameof(TestNotifyPropertyChangedBase.Value2);
            bool callbackInvoked = false;

            target.RegisterPropertyChangedCallback(propertyName, (sender, args) =>
            {
                callbackInvoked = true;
            });

            // Set initial value
            target.Value2 = "Initial Value";
            callbackInvoked = false;

            // Act
            target.Value2 = null;

            // Assert
            Assert.IsTrue(callbackInvoked);
        }

    }
}


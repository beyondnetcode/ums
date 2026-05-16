using Ums.Shell.Ddd.Extensions;
using MediatR;
using Moq;

namespace Ums.Shell.Ddd.Test.Extensions
{
    [TestClass]
    public class MediatorExtensionTest
    {
        private Mock<IMediator> _mediatorMock;

        [TestInitialize]
        public void Setup()
        {
            _mediatorMock = new Mock<IMediator>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DispatchDomainEventsAsync_ShouldThrowIfMediatorIsNull()
        {
            // Arrange
            var entityMock = new Mock<object>().Object;

            // Act
            await MediatorExtension.DispatchDomainEventsAsync(null, entityMock);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DispatchDomainEventsAsync_ShouldThrowIfEntityIsNull()
        {
            // Arrange
            var mediator = new Mock<IMediator>().Object;

            // Act
            await mediator.DispatchDomainEventsAsync(null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using UnifiedFlux.Core;

namespace UnifiedFlux.Core.Tests
{
    public class UnifiedDispatcherTests
    {
        [Fact]
        public async Task Dispatch_Should_Call_Correct_RequestHandler_And_Return_Response()
        {
            // Arrange
            var request = new PingRequest { Message = "Test" };
            var handler = new PingRequestHandler();
            var expectedResponse = $"Pong: {request.Message}";

            // Mock ServiceFactory to return the request handler
            var mockFactory = new Mock<ServiceFactory>();
            mockFactory.Setup(f => f(typeof(IUnifiedRequestHandler<PingRequest, string>)))
                       .Returns(handler);

            var dispatcher = new UnifiedDispatcher(mockFactory.Object, _ => Enumerable.Empty<object>());

            // Act
            var actualResponse = await dispatcher.Dispatch(request);

            // Assert
            Assert.Equal(expectedResponse, actualResponse);
            // Verify ServiceFactory is called only once to resolve the Handler
            mockFactory.Verify(f => f(It.IsAny<Type>()), Times.Once);
        }

        [Fact]
        public async Task Dispatch_Should_Throw_If_No_Handler_Registered()
        {
            // Arrange
            var request = new PingRequest { Message = "Test" };
            
            // Mock ServiceFactory to return null
            var mockFactory = new Mock<ServiceFactory>();
            mockFactory.Setup(f => f(It.IsAny<Type>())).Returns(null);

            var dispatcher = new UnifiedDispatcher(mockFactory.Object, _ => Enumerable.Empty<object>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.Dispatch(request));
        }

        [Fact]
        public async Task Publish_Should_Call_All_Registered_Handlers_In_Parallel()
        {
            // Arrange
            var notification = new UserCreatedNotification { UserId = 1 };

            // Register two synchronous handlers and one asynchronous handler
            var logHandler = new UserLogHandler();
            var emailHandler = new UserEmailHandler();
            var asyncHandler = new AsyncNotificationHandler();

            // 1. Create a list containing handlers and upcast to an object collection
            var handlerList = new List<object> { logHandler, emailHandler, asyncHandler };

            // 2. Mock ServiceFactoryMany to ensure it returns IEnumerable<object>
            var mockFactoryMany = new Mock<ServiceFactoryMany>();
            mockFactoryMany
                // Match requests for generic IEnumerable<IUnifiedNotificationHandler<T>>
                .Setup(f => f(typeof(IEnumerable<IUnifiedNotificationHandler<UserCreatedNotification>>)))
                // Ensure it returns IEnumerable<object> containing our handler instances
                .Returns(handlerList.AsEnumerable());

            var dispatcher = new UnifiedDispatcher(_ => null, mockFactoryMany.Object);

            // Act
            // ... (Act/Assert section remains unchanged)
            var publishTask = dispatcher.Publish(notification);

            // Before waiting for Publish to complete, verify synchronous handlers have run
            // This assertion is correct because even with asynchronous execution, synchronous handlers complete immediately
            Assert.True(logHandler.WasCalled);
            Assert.True(emailHandler.WasCalled);

            await publishTask;

            // Assert
            Assert.True(asyncHandler.Tcs.Task.IsCompletedSuccessfully);
            mockFactoryMany.Verify(f => f(It.IsAny<Type>()), Times.Once);
        }

        [Fact]
        public async Task Publish_Should_Handle_No_Registered_Handlers()
        {
            // Arrange
            var notification = new UserCreatedNotification { UserId = 1 };
            
            // Mock ServiceFactoryMany to return an empty collection
            var mockFactoryMany = new Mock<ServiceFactoryMany>();
            mockFactoryMany.Setup(f => f(It.IsAny<Type>()))
                           .Returns(Enumerable.Empty<object>());

            var dispatcher = new UnifiedDispatcher(_ => null, mockFactoryMany.Object);

            // Act
            // If there are no handlers, Publish should not throw an exception
            var exception = await Record.ExceptionAsync(() => dispatcher.Publish(notification));

            // Assert
            Assert.Null(exception);
            mockFactoryMany.Verify(f => f(It.IsAny<Type>()), Times.Once);
        }
    }
}

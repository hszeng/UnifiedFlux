using System.Threading;
using System.Threading.Tasks;

namespace UnifiedFlux.Core
{
    /// <summary>
    /// Defines the core interface for a mediator, used to send requests and publish notifications.
    /// </summary>
    public interface IUnifiedDispatcher
    {
        /// <summary>
        /// Asynchronously sends a request and waits for a single response.
        /// </summary>
        /// <typeparam name="TResponse">The expected response type.</typeparam>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation, with a result of TResponse.</returns>
        Task<TResponse> Dispatch<TResponse>(IUnifiedRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously publishes a notification, handled by one or more handlers.
        /// </summary>
        /// <param name="notification">The notification object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Publish(IUnifiedNotification notification, CancellationToken cancellationToken = default);
    }
}
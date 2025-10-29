using System.Threading;
using System.Threading.Tasks;

namespace UnifiedFlux.Core
{
    /// <summary>
    /// Marker interface representing a notification or event.
    /// </summary>
    public interface IUnifiedNotification
    {
        // This is a marker interface
    }

    /// <summary>
    /// Defines a handler for processing a specific type of notification.
    /// An IUnifiedNotification can have zero or more IUnifiedNotificationHandler.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to handle.</typeparam>
    public interface IUnifiedNotificationHandler<in TNotification>
        where TNotification : IUnifiedNotification
    {
        /// <summary>
        /// Handles a notification.
        /// </summary>
        /// <param name="notification">Notification object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
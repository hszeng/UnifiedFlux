using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnifiedFlux.Core.Internals;

namespace UnifiedFlux.Core
{
    /// <summary>
    /// Core implementation of IUnifiedDispatcher.
    /// </summary>
    public class UnifiedDispatcher : IUnifiedDispatcher
    {
        private readonly ServiceFactory _serviceFactory;
        private readonly ServiceFactoryMany _serviceFactoryMany;

        // Cache wrappers to improve performance
        private static readonly ConcurrentDictionary<Type, RequestHandlerWrapperBase> _requestHandlers = new ConcurrentDictionary<Type, RequestHandlerWrapperBase>();
        private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapperBase> _notificationHandlers = new ConcurrentDictionary<Type, NotificationHandlerWrapperBase>();

        /// <summary>
        /// Constructs UnifiedDispatcher.
        /// </summary>
        /// <param name="serviceFactory">Delegate for resolving a single service.</param>
        /// <param name="serviceFactoryMany">Delegate for resolving multiple services (IEnumerable<T>).</param>
        public UnifiedDispatcher(ServiceFactory serviceFactory, ServiceFactoryMany serviceFactoryMany)
        {
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _serviceFactoryMany = serviceFactoryMany ?? throw new ArgumentNullException(nameof(serviceFactoryMany));
        }

        public Task<TResponse> Dispatch<TResponse>(IUnifiedRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();

            // 1. Get or create the handler wrapper from the cache
            var handlerWrapper = _requestHandlers.GetOrAdd(requestType,
                t => (RequestHandlerWrapperBase)Activator.CreateInstance(
                    typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse))
                )!); // ! - C# 8/9 non-nullable

            // 2. Use the wrapper to call the Handle method
            return handlerWrapper.Handle(request, cancellationToken, _serviceFactory);
        }

        public async Task Publish(IUnifiedNotification notification, CancellationToken cancellationToken = default)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var notificationType = notification.GetType();

            // 1. Get or create the notification wrapper from the cache
            var handlerWrapper = _notificationHandlers.GetOrAdd(notificationType,
                t => (NotificationHandlerWrapperBase)Activator.CreateInstance(
                    typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType)
                )!); // ! - C# 8/9 non-nullable

            // 2. Use the wrapper to get all handlers and invoke them
            var handlers = handlerWrapper.GetHandlers(_serviceFactoryMany);
            
            var tasks = new List<Task>();
            foreach (var handler in handlers)
            {
                tasks.Add(handler.Handle(notification, cancellationToken));
            }

            // By default, execute in parallel using Task.WhenAll
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
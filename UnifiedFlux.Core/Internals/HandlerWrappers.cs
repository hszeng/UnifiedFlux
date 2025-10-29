using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnifiedFlux.Core.Internals
{
    // --- Request Wrappers ---

    /// <summary>
    /// Non-generic base class wrapper for request handlers.
    /// </summary>
    internal abstract class RequestHandlerWrapperBase
    {
        /// <summary>
        /// Handles the request using a method-level generic parameter.
        /// </summary>
        /// <typeparam name="TMethodResponse">The response type defined by the method call.</typeparam>
        public abstract Task<TMethodResponse> Handle<TMethodResponse>(
            IUnifiedRequest<TMethodResponse> request, 
            CancellationToken cancellationToken, 
            ServiceFactory serviceFactory
        );
    }

    /// <summary>
    /// Generic implementation wrapper for request handlers.
    /// TRequest and TResponse are the class-level generics bound to a specific handler.
    /// </summary>
    internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapperBase
        where TRequest : IUnifiedRequest<TResponse>
    {
        /// <summary>
        /// Overrides the base Handle method.
        /// </summary>
        /// <typeparam name="TMethodResponse">
        /// This type parameter *must* match the base definition.
        /// The implementation casts its class-level TResponse to this TMethodResponse.
        /// </typeparam>
        public override Task<TMethodResponse> Handle<TMethodResponse>(
            IUnifiedRequest<TMethodResponse> request, 
            CancellationToken cancellationToken, 
            ServiceFactory serviceFactory
        )
        {
            // 1. Resolve the actual handler (IUnifiedRequestHandler<TRequest, TResponse>)
            var handler = (IUnifiedRequestHandler<TRequest, TResponse>?)serviceFactory(
                typeof(IUnifiedRequestHandler<TRequest, TResponse>)
            );
            
            if (handler == null)
            {
                throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");
            }

            // 2. Invoke the handler's Handle method.
            // This returns Task<TResponse> (the class-level generic).
            var result = handler.Handle((TRequest)request, cancellationToken);

            // 3. Cast the result.
            // We cast Task<TResponse> (class-level) to Task<TMethodResponse> (method-level).
            // This is safe because the UnifiedDispatcher ensures this wrapper is
            // only created when TResponse == TMethodResponse.
            return (Task<TMethodResponse>)(object)result;
        }
    }

    // --- Notification Wrappers ---
    // (Notification wrappers remain unchanged from the previous answer)

    /// <summary>
    /// Non-generic base class wrapper for notification handlers.
    /// </summary>
    internal abstract class NotificationHandlerWrapperBase
    {
        public abstract IEnumerable<NotificationHandlerExecutor> GetHandlers(ServiceFactoryMany serviceFactoryMany);
    }

/// <summary>
/// Generic implementation wrapper for notification handlers.
/// </summary>
internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapperBase
    where TNotification : IUnifiedNotification
{
    public override IEnumerable<NotificationHandlerExecutor> GetHandlers(ServiceFactoryMany serviceFactoryMany)
    {
        var handlerType = typeof(IUnifiedNotificationHandler<TNotification>);
        var enumerableHandlerType = typeof(IEnumerable<>).MakeGenericType(handlerType);

        // --- 修正后的关键代码 ---
        
        // 1. 调用工厂，它返回 IEnumerable<object> 
        var rawHandlers = serviceFactoryMany(enumerableHandlerType);

        // 2. 使用 .Cast<T>() 安全地将 IEnumerable<object> 转换为目标 IEnumerable<THandler>
        // C# 运行时允许 .Cast<T>() 成功执行，即使 rawHandlers 为空或类型不完全匹配。
        var handlers = rawHandlers
            .Cast<IUnifiedNotificationHandler<TNotification>>();

        // 3. 将它们包装在执行器中
        return handlers.Select(handler => new NotificationHandlerExecutor(handler));
    }
}

    /// <summary>
    /// Wraps a single notification handler to provide a non-generic way to call Handle.
    /// Note: `dynamic` is used here to simplify the call and avoid complex reflection.
    /// </summary>
    internal class NotificationHandlerExecutor
    {
        private readonly object _handler;

        public NotificationHandlerExecutor(object handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task Handle(IUnifiedNotification notification, CancellationToken cancellationToken)
        {
            // We know _handler implements IUnifiedNotificationHandler<TNotification>
            // and notification is of type TNotification
            return ((dynamic)_handler).Handle((dynamic)notification, cancellationToken);
        }
    }
}
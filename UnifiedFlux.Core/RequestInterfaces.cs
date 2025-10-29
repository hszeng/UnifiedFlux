using System.Threading;
using System.Threading.Tasks;

namespace UnifiedFlux.Core
{
    /// <summary>
    /// Marker interface indicating a request that returns a result of type TResponse.
    /// </summary>
    /// <typeparam name="TResponse">The return type of the request.</typeparam>
    public interface IUnifiedRequest<TResponse>
    {
        // This is a marker interface used for type constraints
    }

    /// <summary>
    /// Defines a handler for processing a specific type of request.
    /// An IUnifiedRequest can only have one IUnifiedRequestHandler.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    /// <typeparam name="TResponse">The return type of the request.</typeparam>
    public interface IUnifiedRequestHandler<in TRequest, TResponse>
        where TRequest : IUnifiedRequest<TResponse>
    {
        /// <summary>
        /// Handles a request.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation, with a result of TResponse.</returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
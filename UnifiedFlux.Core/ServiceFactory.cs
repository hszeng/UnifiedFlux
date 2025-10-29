using System;
using System.Collections.Generic;

namespace UnifiedFlux.Core
{
    /// <summary>
    /// Defines a factory delegate for resolving service instances from the service container.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>The service instance.</returns>
    public delegate object ServiceFactory(Type serviceType);

    /// <summary>
    /// Defines a factory delegate for resolving multiple service instances from the service container.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve (usually IEnumerable&lt;T&gt;).</param>
    /// <returns>A collection of service instances.</returns>
    public delegate IEnumerable<object> ServiceFactoryMany(Type serviceType);
}
#  UnifiedFlux

`UnifiedFlux` is a lightweight, zero-dependency .NET library for implementing the Mediator pattern in your application. It helps you decouple components by sending Requests and publishing Notifications, leading to cleaner and more maintainable code.

This project is an independent implementation based on the generic Mediator design pattern and is not related to the code of any other specific library (like MediatR).

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)

##  Core Concepts

`UnifiedFlux` is built around two core concepts:

1.  **Request/Response**:
    * **`IUnifiedRequest<TResponse>`**: A request (Command or Query) that expects a `TResponse` result.
    * **`IUnifiedRequestHandler<TRequest, TResponse>`**: The class that handles the request. **Each request must have exactly one handler**.

2.  **Notification**:
    * **`IUnifiedNotification`**: A notification (or Event) that is "published" and does not expect a return value.
    * **`IUnifiedNotificationHandler<TNotification>`**: The class that responds to the notification. **A notification can have zero or more handlers**.

##  How to Use

`UnifiedFlux` intentionally does not include any DI container-specific registration code. You need to manually register its core service, `IUnifiedDispatcher`, and your handlers into your chosen DI container.

### 1. Register in your DI Container

You need to register:
1.  `IUnifiedDispatcher` and its implementation `UnifiedDispatcher`.
2.  The `ServiceFactory` and `ServiceFactoryMany` delegates used for resolving services.
3.  All your handlers (`IUnifiedRequestHandler` and `IUnifiedNotificationHandler`).

**Example (using `Microsoft.Extensions.DependencyInjection`)**

```csharp
// In Startup.cs or Program.cs
var services = new ServiceCollection();

// 1. Register factory delegates
// (This is the key to decoupling UnifiedFlux from the DI container)
services.AddTransient<UnifiedFlux.Core.ServiceFactory>(provider => provider.GetRequiredService);
services.AddTransient<UnifiedFlux.Core.ServiceFactoryMany>(provider => 
    type => (IEnumerable<object>)provider.GetRequiredServices(type));

// 2. Register the core dispatcher
services.AddTransient<IUnifiedDispatcher, UnifiedDispatcher>();

// 3. Auto-scan and register all handlers (Recommended)
// You might want to use a library like Scrutor to simplify scanning
services.Scan(scan => scan
    .FromAssemblyOf<MyRequest>() // Specify an assembly containing your handlers
    .AddClasses(classes => classes.AssignableTo(typeof(IUnifiedRequestHandler<,>)))
        .AsImplementedInterfaces()
        .WithTransientLifetime()
    .AddClasses(classes => classes.AssignableTo(typeof(IUnifiedNotificationHandler<>)))
        .AsImplementedInterfaces()
        .WithTransientLifetime()
);
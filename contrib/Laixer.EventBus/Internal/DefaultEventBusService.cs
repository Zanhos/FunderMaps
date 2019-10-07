﻿using Laixer.EventBus.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Laixer.EventBus.Internal
{
    /// <summary>
    /// Default implementation of the event bus service.
    /// </summary>
    internal sealed class DefaultEventBusService : EventBusService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<EventBusServiceOptions> _options;
        private readonly ILogger<DefaultEventBusService> _logger;

        public DefaultEventBusService(
            IServiceScopeFactory scopeFactory,
            IOptions<EventBusServiceOptions> options,
            ILogger<DefaultEventBusService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Return list of derived interfaces from <typeparamref name="TInterface"/> exculding itself.
        /// </summary>
        /// <typeparam name="TInterface">Interface to derive from.</typeparam>
        /// <param name="obj">Object to test interfaces.</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetDerivedInterfaces<TInterface>(object obj) => obj.GetType()
            .GetInterfaces()
            .Where(type => type != typeof(TInterface) && typeof(TInterface).IsAssignableFrom(type));

        /// <summary>
        /// Checks if the parameter of the given method contains the generic type.
        /// </summary>
        /// <param name="methodInfo">Method to test.</param>
        /// <param name="type">Type to search for.</param>
        /// <returns>True if found, false otherwise.</returns>
        private static bool ContainsEventHandlerContextGenericType(MethodInfo methodInfo, Type type) => methodInfo.GetParameters()
            .Any(s => s.ParameterType.GenericTypeArguments.Any(t => t == type));

        /// <summary>
        /// Runs the provided health checks and returns the aggregated status
        /// </summary>
        /// <param name="predicate">
        /// A predicate that can be used to include health checks based on user-defined criteria.
        /// </param>
        /// <param name="event">Event to fire.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the health checks.</param>
        public override async Task FireEventAsync(Func<EventHandlerRegistration, bool> predicate, IEvent @event, CancellationToken cancellationToken = default)
        {
            var registrations = _options.Value.Registrations;
            if (registrations.Count == 0) { return; }

            // Filter the list (optional).
            if (predicate != null)
            {
                registrations = registrations.Where(predicate).ToArray();
            }

            int index = 0;

            // Fire all handlers for all registered events
            foreach (Type type in GetDerivedInterfaces<IEvent>(@event))
            {
                var qualifiedHandlerRegistrations = registrations.Where(r => r.EventInterfaceType == type);
                if (qualifiedHandlerRegistrations.Count() == 0) { continue; }

                var tasks = new Task[qualifiedHandlerRegistrations.Count()];
                using (var scope = _scopeFactory.CreateScope())
                {
                    foreach (EventHandlerRegistration handlerRegistration in qualifiedHandlerRegistrations)
                    {
                        Type eventHandlerType = handlerRegistration.ImplementationType;
                        IEventHandler eventHandler = (IEventHandler)ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, eventHandlerType);

                        // Find the method witch accepts 'type' as argument
                        var handleEventAsyncMethod = eventHandler.GetType()
                            .GetMethods()
                            .Where(r => r.Name == "HandleEventAsync")
                            .Where(r => ContainsEventHandlerContextGenericType(r, type))
                            .First();

                        if (handleEventAsyncMethod == null)
                        {
                            throw new Exception($"{eventHandler.GetType().Name} does not implement IEventHandler<>");
                        }

                        // Create the even handler context
                        Type handlerContextType = typeof(EventHandlerContext<>).MakeGenericType(type);
                        EventHandlerContext handlerContext = (EventHandlerContext)Activator.CreateInstance(handlerContextType);
                        handlerContext.Registration = handlerRegistration;

                        var eventProperty = handlerContext.GetType().GetProperty("Event");
                        if (eventProperty != null)
                        {
                            eventProperty.SetValue(handlerContext, @event);
                        }

                        tasks[index++] = FireEventHandlerAsync(handleEventAsyncMethod, handlerContext, eventHandler, cancellationToken);
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task FireEventHandlerAsync(MethodInfo methodInfo, EventHandlerContext context, IEventHandler handler, CancellationToken cancellationToken)
        {
            await Task.Yield();

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await (Task)methodInfo.Invoke(handler, new object[] { context, cancellationToken });
            }
            catch (Exception ex) when (ex as OperationCanceledException == null)
            {
                throw;
            }
        }
    }
}

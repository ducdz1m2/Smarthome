using Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }

    public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Get all handlers for this event type
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler == null) continue;

            var method = handlerType.GetMethod("HandleAsync");
            if (method != null)
            {
                await ((Task)method.Invoke(handler, new object[] { domainEvent, cancellationToken })!)!;
            }
        }
    }
}

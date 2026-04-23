using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Events;

namespace Application.EventHandlers;

/// <summary>
/// Handler for synchronizing installation booking status changes to order and warranty request status
/// </summary>
public class InstallationStatusSyncHandler :
    IDomainEventHandler<InstallationBookingConfirmedEvent>,
    IDomainEventHandler<InstallationCancelledEvent>,
    IDomainEventHandler<InstallationCompletedEvent>
{
    private readonly IInstallationBookingRepository _installationBookingRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IWarrantyRequestRepository _warrantyRequestRepository;

    public InstallationStatusSyncHandler(
        IInstallationBookingRepository installationBookingRepository,
        IOrderRepository orderRepository,
        IWarrantyRequestRepository warrantyRequestRepository)
    {
        _installationBookingRepository = installationBookingRepository;
        _orderRepository = orderRepository;
        _warrantyRequestRepository = warrantyRequestRepository;
    }

    public async Task HandleAsync(InstallationBookingConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var booking = await _installationBookingRepository.GetByIdAsync(domainEvent.BookingId);
        if (booking == null) return;

        Console.WriteLine($"[InstallationStatusSyncHandler] BookingConfirmedEvent START - BookingId: {booking.Id}, Status: {booking.Status}, IsWarranty: {booking.IsWarranty}");

        // Update order status to TechnicianAssigned
        var order = await _orderRepository.GetByIdAsync(booking.OrderId);
        if (order != null)
        {
            Console.WriteLine($"[InstallationStatusSyncHandler] Before update order - Order ID: {order.Id}, Status: {order.Status}");
            // Skip update if order is already completed
            if (order.Status == OrderStatus.Completed)
            {
                Console.WriteLine($"[InstallationStatusSyncHandler] Order already completed, skipping status update");
            }
            else
            {
                order.UpdateStatusFromInstallation(OrderStatus.TechnicianAssigned);
                await _orderRepository.SaveChangesAsync();
                Console.WriteLine($"[InstallationStatusSyncHandler] After update order - Order ID: {order.Id}, Status: {order.Status}");
            }
        }

        // Update warranty request status to InProgress
        if (booking.IsWarranty)
        {
            try
            {
                Console.WriteLine($"[InstallationStatusSyncHandler] Looking for warranty request for booking {booking.Id}");
                var warrantyRequest = await _warrantyRequestRepository.GetByBookingIdAsync(booking.Id);
                if (warrantyRequest != null)
                {
                    Console.WriteLine($"[InstallationStatusSyncHandler] Found warranty request - ID: {warrantyRequest.Id}, Status: {warrantyRequest.Status}");
                    // If still pending, approve first, then start
                    if (warrantyRequest.Status == WarrantyRequestStatus.Pending)
                    {
                        Console.WriteLine($"[InstallationStatusSyncHandler] Calling Approve() on warranty request");
                        warrantyRequest.Approve();
                        Console.WriteLine($"[InstallationStatusSyncHandler] Approved warranty request, new status: {warrantyRequest.Status}");
                    }
                    if (warrantyRequest.Status == WarrantyRequestStatus.Approved)
                    {
                        Console.WriteLine($"[InstallationStatusSyncHandler] Calling Start() on warranty request");
                        warrantyRequest.Start();
                        Console.WriteLine($"[InstallationStatusSyncHandler] Started warranty request, new status: {warrantyRequest.Status}");
                    }
                    _warrantyRequestRepository.Update(warrantyRequest);
                    await _warrantyRequestRepository.SaveChangesAsync();
                    Console.WriteLine($"[InstallationStatusSyncHandler] Saved warranty request - ID: {warrantyRequest.Id}, Status: {warrantyRequest.Status}");
                }
                else
                {
                    Console.WriteLine($"[InstallationStatusSyncHandler] WARNING: Warranty request not found for booking {booking.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InstallationStatusSyncHandler] ERROR updating warranty request: {ex.Message}");
                Console.WriteLine($"[InstallationStatusSyncHandler] Stack trace: {ex.StackTrace}");
            }
        }
        Console.WriteLine($"[InstallationStatusSyncHandler] BookingConfirmedEvent END - BookingId: {booking.Id}");
    }

    public async Task HandleAsync(InstallationCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var booking = await _installationBookingRepository.GetByIdAsync(domainEvent.BookingId);
        if (booking == null) return;

        Console.WriteLine($"[InstallationStatusSyncHandler] InstallationCancelledEvent - BookingId: {booking.Id}, Status: {booking.Status}, IsWarranty: {booking.IsWarranty}");

        // Update order status to Cancelled
        var order = await _orderRepository.GetByIdAsync(booking.OrderId);
        if (order != null)
        {
            Console.WriteLine($"[InstallationStatusSyncHandler] Before update order - Order ID: {order.Id}, Status: {order.Status}");
            order.UpdateStatusFromInstallation(OrderStatus.Cancelled);
            await _orderRepository.SaveChangesAsync();
            Console.WriteLine($"[InstallationStatusSyncHandler] After update order - Order ID: {order.Id}, Status: {order.Status}");
        }

        // Update warranty request status to Rejected
        if (booking.IsWarranty)
        {
            var warrantyRequest = await _warrantyRequestRepository.GetByBookingIdAsync(booking.Id);
            if (warrantyRequest != null)
            {
                Console.WriteLine($"[InstallationStatusSyncHandler] Before update warranty request - ID: {warrantyRequest.Id}, Status: {warrantyRequest.Status}");
                warrantyRequest.Reject(domainEvent.Reason);
                await _warrantyRequestRepository.SaveChangesAsync();
                Console.WriteLine($"[InstallationStatusSyncHandler] After update warranty request - ID: {warrantyRequest.Id}, Status: {warrantyRequest.Status}");
            }
        }
    }

    public async Task HandleAsync(InstallationCompletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var booking = await _installationBookingRepository.GetByIdAsync(domainEvent.BookingId);
        if (booking == null) return;

        Console.WriteLine($"[InstallationStatusSyncHandler] InstallationCompletedEvent - BookingId: {booking.Id}, Status: {booking.Status}, IsWarranty: {booking.IsWarranty}");

        // Update order status to Completed
        var order = await _orderRepository.GetByIdAsync(booking.OrderId);
        if (order != null)
        {
            Console.WriteLine($"[InstallationStatusSyncHandler] Before update order - Order ID: {order.Id}, Status: {order.Status}");
            order.UpdateStatusFromInstallation(OrderStatus.Completed);
            await _orderRepository.SaveChangesAsync();
            Console.WriteLine($"[InstallationStatusSyncHandler] After update order - Order ID: {order.Id}, Status: {order.Status}");
        }

        // Update warranty request status to Completed
        if (booking.IsWarranty)
        {
            try
            {
                var warrantyRequest = await _warrantyRequestRepository.GetByBookingIdAsync(booking.Id);
                if (warrantyRequest != null)
                {
                    Console.WriteLine($"[InstallationStatusSyncHandler] Before update warranty request - ID: {warrantyRequest.Id}, Status: {warrantyRequest.Status}");
                    warrantyRequest.Complete(domainEvent.Notes);
                    await _warrantyRequestRepository.SaveChangesAsync();
                    Console.WriteLine($"[InstallationStatusSyncHandler] After update warranty request - ID: {warrantyRequest.Id}, Status: {warrantyRequest.Status}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InstallationStatusSyncHandler] Error updating warranty request: {ex.Message}");
                Console.WriteLine($"[InstallationStatusSyncHandler] Stack trace: {ex.StackTrace}");
            }
        }
    }
}

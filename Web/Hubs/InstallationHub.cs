using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

public class InstallationHub : Hub
{
    public async Task JoinTechnicianGroup(int technicianId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tech_{technicianId}");
    }

    public async Task JoinAdminInstallationGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin_installation");
    }

    public async Task JoinAdminOrdersGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin_orders");
    }

    public async Task JoinTechnicianPoolGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "technician_pool");
    }

    public async Task LeaveTechnicianPoolGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "technician_pool");
    }

    public async Task NotifyNewBookingToTechnician(int technicianId, object booking)
    {
        await Clients.Group($"tech_{technicianId}")
            .SendAsync("NewBookingAssigned", booking);
    }

    public async Task NotifyBookingCancelled(int bookingId, string reason)
    {
        await Clients.Group($"booking_{bookingId}")
            .SendAsync("BookingCancelled", new { BookingId = bookingId, Reason = reason });
        
        await Clients.Group("admin_installation")
            .SendAsync("BookingCancelled", new { BookingId = bookingId, Reason = reason });
    }

    public async Task NotifyBookingRescheduled(int bookingId, object newSlot)
    {
        await Clients.Group($"booking_{bookingId}")
            .SendAsync("BookingRescheduled", new { BookingId = bookingId, NewSlot = newSlot });
    }

    public async Task NotifyTechnicianCheckedIn(int bookingId, DateTime checkInTime)
    {
        await Clients.Group($"booking_{bookingId}")
            .SendAsync("TechnicianCheckedIn", new { BookingId = bookingId, CheckedInAt = checkInTime });
        
        await Clients.Group("admin_installation")
            .SendAsync("TechnicianCheckedIn", new { BookingId = bookingId, CheckedInAt = checkInTime });
    }

    public async Task NotifyJobStarted(int bookingId, DateTime startTime)
    {
        await Clients.Group($"booking_{bookingId}")
            .SendAsync("JobStarted", new { BookingId = bookingId, StartedAt = startTime });
    }

    public async Task NotifyJobCompleted(int bookingId, DateTime completedTime, List<string> photos)
    {
        await Clients.Group($"booking_{bookingId}")
            .SendAsync("JobCompleted", new { 
                BookingId = bookingId, 
                CompletedAt = completedTime, 
                Photos = photos 
            });
        
        await Clients.Group("admin_installation")
            .SendAsync("JobCompleted", new { 
                BookingId = bookingId, 
                CompletedAt = completedTime 
            });
    }

    public async Task NotifySlotAvailable(int slotId, object slot)
    {
        await Clients.Group("admin_installation")
            .SendAsync("SlotAvailable", slot);
    }

    public async Task NotifyLowSlotAvailability(string district, DateTime date, int remainingSlots)
    {
        await Clients.Group("admin_installation")
            .SendAsync("LowSlotAvailability", new { 
                District = district, 
                Date = date, 
                RemainingSlots = remainingSlots 
            });
    }
}

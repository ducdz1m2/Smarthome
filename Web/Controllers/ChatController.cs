using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IFileUploadService _fileUploadService;

    public ChatController(IChatService chatService, IFileUploadService fileUploadService)
    {
        _chatService = chatService;
        _fileUploadService = fileUploadService;
    }

    [HttpGet("rooms")]
    [Authorize]
    public async Task<ActionResult<List<ChatRoomResponse>>> GetUserChatRooms([FromQuery] UserType userType)
    {
        // Get user ID from JWT token (simplified - in production use proper claim extraction)
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var rooms = await _chatService.GetUserChatRoomsAsync(userId, userType);
        return Ok(rooms);
    }

    [HttpGet("rooms/installation")]
    [Authorize]
    public async Task<ActionResult<List<ChatRoomResponse>>> GetInstallationChatRooms()
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var rooms = await _chatService.GetInstallationChatRoomsAsync(userId);
        return Ok(rooms);
    }

    [HttpGet("rooms/customer-installation")]
    [Authorize]
    public async Task<ActionResult<List<ChatRoomResponse>>> GetCustomerInstallationChats()
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var rooms = await _chatService.GetCustomerInstallationChatsAsync(userId);
        return Ok(rooms);
    }

    [HttpGet("rooms/{id}")]
    [Authorize]
    public async Task<ActionResult<ChatRoomResponse>> GetChatRoom(int id)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var room = await _chatService.GetChatRoomByIdAsync(id, userId);
        if (room == null)
            return NotFound();

        return Ok(room);
    }

    [HttpGet("rooms/{id}/messages")]
    [Authorize]
    public async Task<ActionResult<List<ChatMessageResponse>>> GetChatMessages(int id, [FromQuery] UserType userType, [FromQuery] int limit = 50)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var messages = await _chatService.GetChatMessagesAsync(id, userId, userType, limit);
        return Ok(messages);
    }

    [HttpPost("rooms/installation")]
    [Authorize]
    public async Task<ActionResult<int>> CreateInstallationChat([FromBody] CreateInstallationChatRequest request)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var chatRoomId = await _chatService.CreateInstallationChatAsync(request.CustomerId, request.TechnicianId, request.InstallationId);
        return Ok(chatRoomId);
    }

    [HttpPost("rooms/support")]
    [Authorize]
    public async Task<ActionResult<int>> CreateSupportChat([FromBody] CreateSupportChatRequest request)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var chatRoomId = await _chatService.CreateSupportChatAsync(request.CustomerId, request.OrderId, request.InstallationId, request.WarrantyClaimId);
        return Ok(chatRoomId);
    }

    [HttpPost("rooms/{id}/messages")]
    [Authorize]
    public async Task<ActionResult<int>> SendMessage(int id, [FromBody] SendMessageRequest request)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var userTypeStr = User.FindFirst("userType")?.Value ?? "Customer";
        if (!Enum.TryParse<UserType>(userTypeStr, out var userType))
            userType = UserType.Customer;

        var messageId = await _chatService.SendMessageAsync(id, userId, userType, request);
        return Ok(messageId);
    }

    [HttpPut("rooms/{id}/read")]
    [Authorize]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        await _chatService.MarkChatAsReadAsync(id, userId);
        return Ok();
    }

    [HttpDelete("rooms/{id}")]
    [Authorize]
    public async Task<ActionResult> CloseChatRoom(int id)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        await _chatService.CloseChatRoomAsync(id, userId);
        return Ok();
    }

    [HttpPost("rooms/{id}/assign-technician")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> AssignTechnician(int id, [FromBody] AssignTechnicianRequest request)
    {
        await _chatService.AssignTechnicianAsync(id, request.TechnicianId);
        return Ok();
    }

    [HttpPost("rooms/{id}/assign-admin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> AssignAdmin(int id, [FromBody] AssignAdminRequest request)
    {
        await _chatService.AssignAdminAsync(id, request.AdminId);
        return Ok();
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<ActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        try
        {
            var tempUrl = await _fileUploadService.UploadTempAsync(file);
            return Ok(new
            {
                fileUrl = tempUrl,
                fileName = file.FileName,
                fileType = file.ContentType,
                fileSize = file.Length
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Upload failed: {ex.Message}");
        }
    }
}

// Request DTOs
public class CreateInstallationChatRequest
{
    public int CustomerId { get; set; }
    public int TechnicianId { get; set; }
    public int InstallationId { get; set; }
}

public class CreateSupportChatRequest
{
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public int? InstallationId { get; set; }
    public int? WarrantyClaimId { get; set; }
}

public class AssignTechnicianRequest
{
    public int TechnicianId { get; set; }
}

public class AssignAdminRequest
{
    public int AdminId { get; set; }
}

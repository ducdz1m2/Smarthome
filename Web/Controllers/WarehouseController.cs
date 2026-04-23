using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Services;
using Domain.Entities.Inventory;
using Domain.Enums;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly Application.Interfaces.Repositories.IWarehouseRepository _warehouseRepository;
        private readonly IInventoryService _inventoryService;

        public WarehouseController(
            Application.Interfaces.Repositories.IWarehouseRepository warehouseRepository,
            IInventoryService inventoryService)
        {
            _warehouseRepository = warehouseRepository;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var warehouses = await _warehouseRepository.GetAllAsync();
                var response = warehouses.Select(w => new
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Address = w.Address?.ToFullString() ?? "",
                    Phone = w.Phone?.Value ?? "",
                    ManagerName = w.ManagerName ?? "",
                    IsActive = w.IsActive
                }).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transfer/initiate")]
        public async Task<ActionResult> InitiateTransfer([FromBody] InitiateTransferRequest request)
        {
            try
            {
                var transfer = await _inventoryService.InitiateTransferAsync(
                    request.FromWarehouseId,
                    request.ToWarehouseId,
                    request.ProductQuantities,
                    request.Reason);

                return Ok(new { transferId = transfer.Id, message = "Đã tạo phiếu chuyển kho thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transfer/execute")]
        public async Task<ActionResult> ExecuteTransfer([FromBody] ExecuteTransferRequest request)
        {
            try
            {
                await _inventoryService.ExecuteTransferAsync(request.TransferId);
                return Ok(new { message = "Đã thực hiện chuyển kho thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transfer/cancel")]
        public async Task<ActionResult> CancelTransfer([FromBody] CancelTransferRequest request)
        {
            try
            {
                await _inventoryService.CancelTransferAsync(request.TransferId, request.Reason);
                return Ok(new { message = "Đã hủy phiếu chuyển kho thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("transfer")]
        public async Task<ActionResult> GetTransfers([FromQuery] int? warehouseId, [FromQuery] string? status)
        {
            try
            {
                WarehouseTransferStatus? statusEnum = null;
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<WarehouseTransferStatus>(status, true, out var parsedStatus))
                {
                    statusEnum = parsedStatus;
                }

                var transfers = await _inventoryService.GetTransfersAsync(warehouseId, statusEnum);
                var response = transfers.Select(t => new WarehouseTransferResponse
                {
                    Id = t.Id,
                    FromWarehouseId = t.FromWarehouseId,
                    FromWarehouseName = t.FromWarehouse?.Name ?? "",
                    FromWarehouseCode = t.FromWarehouse?.Code ?? "",
                    ToWarehouseId = t.ToWarehouseId,
                    ToWarehouseName = t.ToWarehouse?.Name ?? "",
                    ToWarehouseCode = t.ToWarehouse?.Code ?? "",
                    ProductId = t.ProductId,
                    ProductName = "", // Need to load product
                    ProductSku = "",
                    Quantity = t.Quantity,
                    Reason = t.Reason,
                    Status = t.Status.ToString(),
                    ExecutedAt = t.ExecutedAt,
                    CreatedAt = t.CreatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

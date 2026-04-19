using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InstallationController : ControllerBase
    {
        private readonly IInstallationService _installationService;
        private readonly IProductWarehouseRepository _productWarehouseRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IProductRepository _productRepository;

        public InstallationController(
            IInstallationService installationService,
            IProductWarehouseRepository productWarehouseRepository,
            IWarehouseRepository warehouseRepository,
            IProductRepository productRepository)
        {
            _installationService = installationService;
            _productWarehouseRepository = productWarehouseRepository;
            _warehouseRepository = warehouseRepository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Get all warehouses with available stock for a list of products
        /// </summary>
        [HttpGet("warehouses/for-products")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult<List<WarehouseStockForTechnicianResponse>>> GetWarehousesForProducts(
            [FromQuery] List<int> productIds)
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            var result = new List<WarehouseStockForTechnicianResponse>();

            foreach (var warehouse in warehouses.Where(w => w.IsActive))
            {
                var warehouseStocks = await _productWarehouseRepository.GetByWarehouseAsync(warehouse.Id);
                var availableProducts = new List<ProductStockForTechnician>();

                foreach (var productId in productIds)
                {
                    var stock = warehouseStocks.FirstOrDefault(s => s.ProductId == productId);
                    if (stock != null && stock.GetAvailableStock() > 0)
                    {
                        var product = await _productRepository.GetByIdAsync(productId);
                        availableProducts.Add(new ProductStockForTechnician
                        {
                            ProductId = productId,
                            ProductName = product?.Name ?? $"Sản phẩm #{productId}",
                            Sku = product?.Sku.Value ?? "",
                            VariantId = stock.VariantId,
                            AvailableStock = stock.GetAvailableStock(),
                            ReservedStock = stock.ReservedQuantity
                        });
                    }
                }

                if (availableProducts.Any())
                {
                    result.Add(new WarehouseStockForTechnicianResponse
                    {
                        WarehouseId = warehouse.Id,
                        WarehouseName = warehouse.Name,
                        WarehouseCode = warehouse.Code,
                        WarehouseAddress = warehouse.Address.ToFullString(),
                        AvailableProducts = availableProducts
                    });
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Get available stock for a specific product across all warehouses
        /// </summary>
        [HttpGet("stock/{productId}")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult<List<ProductStockForTechnician>>> GetProductStockAcrossWarehouses(int productId)
        {
            var stocks = await _productWarehouseRepository.GetByProductAsync(productId);
            var product = await _productRepository.GetByIdAsync(productId);
            var warehouses = await _warehouseRepository.GetAllAsync();

            var result = stocks
                .Where(s => s.GetAvailableStock() > 0)
                .Select(s => new ProductStockForTechnician
                {
                    ProductId = productId,
                    ProductName = product?.Name ?? $"Sản phẩm #{productId}",
                    Sku = product?.Sku.Value ?? "",
                    VariantId = s.VariantId,
                    VariantName = s.VariantId.HasValue ? "Variant " + s.VariantId : null,
                    AvailableStock = s.GetAvailableStock(),
                    ReservedStock = s.ReservedQuantity
                })
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Prepare materials from warehouse for an installation booking
        /// </summary>
        [HttpPost("{bookingId}/prepare-materials")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> PrepareMaterials(int bookingId, [FromBody] PrepareMaterialsRequest request)
        {
            try
            {
                await _installationService.PrepareMaterialsFromWarehouseAsync(bookingId, request);
                return Ok(new { message = "Đã chuẩn bị vật tư từ kho thành công" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Return unused materials to warehouse after installation
        /// </summary>
        [HttpPost("{bookingId}/return-materials")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> ReturnMaterials(int bookingId, [FromBody] List<MaterialReturnInfo> returns)
        {
            try
            {
                await _installationService.ReturnMaterialsToWarehouseAsync(bookingId, returns);
                return Ok(new { message = "Đã trả vật tư về kho thành công" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get installation booking details
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult<InstallationBookingResponse>> GetById(int id)
        {
            var booking = await _installationService.GetByIdAsync(id);
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        /// <summary>
        /// Add a single material to booking (with warehouse dispatch)
        /// </summary>
        [HttpPost("{bookingId}/materials")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> AddMaterial(int bookingId, [FromBody] AddInstallationMaterialRequest request)
        {
            try
            {
                await _installationService.AddMaterialAsync(bookingId, request);
                return Ok(new { message = "Đã thêm vật tư thành công" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Accept booking as technician
        /// </summary>
        [HttpPost("{bookingId}/accept")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> AcceptBooking(int bookingId, [FromQuery] int technicianId)
        {
            try
            {
                await _installationService.AcceptBookingAsync(bookingId, technicianId);
                return Ok(new { message = "Đã tiếp nhận lịch lắp đặt" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reject booking as technician
        /// </summary>
        [HttpPost("{bookingId}/reject")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> RejectBooking(int bookingId, [FromQuery] int technicianId, [FromBody] RejectBookingRequest request)
        {
            try
            {
                await _installationService.RejectBookingAsync(bookingId, technicianId, request);
                return Ok(new { message = "Đã từ chối lịch lắp đặt" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Start travel to installation location
        /// </summary>
        [HttpPost("{bookingId}/start-travel")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> StartTravel(int bookingId)
        {
            try
            {
                await _installationService.StartTravelAsync(bookingId);
                return Ok(new { message = "Đã bắt đầu di chuyển" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Start installation at customer location
        /// </summary>
        [HttpPost("{bookingId}/start-installation")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> StartInstallation(int bookingId)
        {
            try
            {
                await _installationService.StartInstallationAsync(bookingId);
                return Ok(new { message = "Đã bắt đầu lắp đặt" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete installation
        /// </summary>
        [HttpPost("{bookingId}/complete")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<ActionResult> Complete(int bookingId, [FromBody] CompleteInstallationRequest request)
        {
            try
            {
                await _installationService.CompleteAsync(bookingId, request);
                return Ok(new { message = "Đã hoàn thành lắp đặt" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Customer reschedule booking (max 1 time)
        /// </summary>
        [HttpPost("{bookingId}/customer-reschedule")]
        [Authorize]
        public async Task<ActionResult> CustomerReschedule(int bookingId, [FromBody] RescheduleInstallationRequest request)
        {
            try
            {
                await _installationService.CustomerRescheduleAsync(bookingId, request);
                return Ok(new { message = "Đã đổi lịch thành công" });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

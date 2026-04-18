using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities.Inventory;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly Application.Interfaces.Repositories.IWarehouseRepository _warehouseRepository;

        public WarehouseController(Application.Interfaces.Repositories.IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
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
    }
}

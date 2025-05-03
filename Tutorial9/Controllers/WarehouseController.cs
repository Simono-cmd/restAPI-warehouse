using Microsoft.AspNetCore.Mvc;
using Tutorial9.Models;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[Route("api/")]
[ApiController]

public class WarehouseController : Controller
{
    private readonly IDbService _dbService;

    public WarehouseController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost("add/product/{productId}/warehouse/{warehouseId}")]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] Product_Warehouse_POST request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than 0");
        }

        Product? product = await _dbService.GetProductById(request.IdProduct);
        if (product == null)
        {
            return NotFound("Product not found");
        }
        
        Warehouse? warehouse = await _dbService.GetWarehouseById(request.IdWarehouse);
        if (warehouse == null)
        {
            return NotFound("Warehouse not found");
        }

        // var result = await _dbService.AddProductToWarehouse
        return Ok(new
        {
            message = $"Added {product.Name} to warehouse {warehouse.Name}",
          //  IdProductWarehouse = result
        
        });
    }
    
    
}
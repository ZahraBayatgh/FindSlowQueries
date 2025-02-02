using FindSlowQueries.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FindSlowQueries.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            await Task.Delay(100);
            var orders = await _context.Orders.Include(o => o.Items)
                .ToListAsync();
            var ordersResult = orders.Take(10).ToList();
            return Ok(ordersResult);
        }

        [HttpGet("search-orders/{productName}")]
        public async Task<IActionResult> SearchOrders(string productName)
        {
            var orders = await _context.Orders
                .Where(o => o.Items.Any(i => i.ProductName.Contains(productName)))
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("large-orders")]
        public async Task<IActionResult> GetLargeOrders()
        {
            var orders = await _context.Orders
                .Where(o => o.Items.Count > 50)
                .ToListAsync();

            return Ok(orders);
        }
    }

}

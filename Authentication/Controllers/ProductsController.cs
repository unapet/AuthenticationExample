using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Authentication.Data;
using Authentication.Models.Products;

namespace Authentication.Controllers
{
    [ApiController]
	[Route("[controller]")]
	[Authorize]
	public class ProductsController : ControllerBase
	{
		private readonly DataContext _context;

		public ProductsController(DataContext context)
		{
			_context = context;
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetProducts()
		{
			var products = await _context.Products.ToListAsync();
			return Ok(products);
		}

		[HttpPost]
		[Authorize(Roles = "User")]
		public async Task<IActionResult> AddProduct(ProductRequest productRequest)
		{
			if (productRequest == null) 
			{
				return BadRequest("Body cannot be null");
			}

			_context.Products.Add(new Product() { Name = productRequest.Name });
			await _context.SaveChangesAsync();

			return Ok("You successfuly added a product.");
		}
	}
}

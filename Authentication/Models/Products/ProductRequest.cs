using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.Products
{
	public class ProductRequest
	{
		[Required]
		public string Name {  get; set; } = string.Empty;
	}
}

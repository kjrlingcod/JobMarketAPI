using JobMarketApp.API.DTO.Customers;
using Microsoft.AspNetCore.Mvc;

namespace JobMarketApp.API.Controllers
{
    public class CustomersController : BaseController
    {
        private readonly ICustomerService _customerService;
        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        [HttpGet]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] string? term,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var customers = await _customerService.SearchAsync(term, page, pageSize);

            return Ok(customers);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);

            if (customer is null)
                return NotFound();

            return Ok(customer);
        }
    }
}

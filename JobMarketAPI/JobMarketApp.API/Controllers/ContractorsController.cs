using JobMarketApp.API.DTO.Contractors;
using Microsoft.AspNetCore.Mvc;

namespace JobMarketApp.API.Controllers
{
    public class ContractorsController : BaseController
    {
        private readonly IContractorService _contractorService;

        public ContractorsController(IContractorService contractorService)
        {
            _contractorService = contractorService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] string? term,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        { 
            var contractors = await _contractorService.SearchAsync(term, page, pageSize);

            return Ok(contractors);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var contractor = await _contractorService.GetByIdAsync(id);

            if (contractor is null)
                return NotFound();

            return Ok(contractor);
        }
    }
}

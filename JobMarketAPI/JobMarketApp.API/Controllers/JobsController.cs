using JobMarketApp.API.DTO.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace JobMarketApp.API.Controllers
{
    public class JobsController : BaseController
    {
        private readonly IJobService _jobService;
        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var job = await _jobService.GetPaginatedAsync(page, pageSize);
            return Ok(job);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var job = await _jobService.GetByIdAsync(id);

            if (job is null)
                return NotFound();

            return Ok(job);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateJobDto jobDto)
        {
            var created = await _jobService.CreateAsync(jobDto);

            return Ok(created);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateJobDto jobDto)
        {
            var updated = await _jobService.UpdateAsync(id, jobDto);

            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var deleted = await _jobService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}

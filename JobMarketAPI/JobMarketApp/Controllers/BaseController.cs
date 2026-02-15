using Microsoft.AspNetCore.Mvc;

namespace JobMarketApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {

    }
}

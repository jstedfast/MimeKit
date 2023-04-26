namespace Gmsl.WebApi
{
    using Microsoft.AspNetCore.Mvc;

    [Route("/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return this.Ok();
        }
    }
}

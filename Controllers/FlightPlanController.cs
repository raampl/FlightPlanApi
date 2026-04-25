using Microsoft.AspNetCore.Mvc;

namespace FlightPlanApi.Controllers
{
    [ApiController]
    [Route("[api/v1/flightplan]")]
    public class FlightPlanController : ControllerBase
    {
        private readonly ILogger<FlightPlanController> _logger;

        public FlightPlanController(ILogger<FlightPlanController> logger)
        {
            _logger = logger;
        }        
    }
}
using FlightPlanApi.Data;
using FlightPlanApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlanApi.Controllers
{
    [ApiController]
    [Route("api/v1/flightplan")]
    public class FlightPlanController : ControllerBase
    {
        private readonly ILogger<FlightPlanController> _logger;
        private IDatabaseAdapter _database;

        public FlightPlanController(ILogger<FlightPlanController> logger, IDatabaseAdapter database)
        {
            _logger = logger;
            _database = database;
        }

        [HttpGet]
        public async Task<IActionResult> FlightPlanList()
        {
            var flightPlanList = await _database.GetAllFlightPlans();
            if (flightPlanList.Count == 0)
            {
                return NoContent();
            }

            return Ok(flightPlanList);
        }

        [HttpGet]
        [Route("{flightPlanId}")]
        public async Task<IActionResult> GetFlightFlightPlanById(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);

            if (flightPlan is null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return Ok(flightPlan);
        }

        [HttpPost]
        [Route("file")]
        public async Task<IActionResult> FileFlightPlan(FlightPlan flightPlan)
        {
            var transactionResult = await _database.FileFlightPlan(flightPlan);
            return transactionResult switch
            {
                TransactionResult.Success => Ok(),
                TransactionResult.BadRequest => StatusCode(StatusCodes.Status400BadRequest),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFlightPlan(FlightPlan flightPlan)
        {
            var updateResult = await _database.UpdateFlightPlan(flightPlan.FlightPlanId, flightPlan);
            return updateResult switch
            {
                TransactionResult.Success => Ok(),
                TransactionResult.NotFound => StatusCode(StatusCodes.Status404NotFound),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpDelete]
        [Route("{flightPlanId}")]
        public async Task<IActionResult> DeleteFlightPlan(string flightPlanId)
        {
            var result = await _database.DeleteFlightPlanById(flightPlanId);
            if (result)
            {
                return Ok();
            }

            return StatusCode(StatusCodes.Status404NotFound);
        }
        
        [HttpGet]
        [Route("airport/departure/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanDepartureAirport(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);
            
            if (flightPlan == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return Ok(flightPlan.DepartureAirport);
        }

        [HttpGet]
        [Route("route/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanRoute(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);

            if (flightPlan is null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return Ok(flightPlan.Route);
        }

        [HttpGet]
        [Route("time/enroute/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanTimeEnroute(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);
            if (flightPlan == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            
            var estimatedTimeEnroute = flightPlan.ArrivalTime - flightPlan.DepartureTime;

            return Ok(estimatedTimeEnroute);
        }
    }
}
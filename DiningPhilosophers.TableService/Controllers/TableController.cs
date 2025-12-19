using DiningPhilosophers.Contracts;
using DiningPhilosophers.TableService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiningPhilosophers.TableService.Controllers
{
    [ApiController]
    [Route("api")]
    public class TableController : ControllerBase
    {
        private readonly IForkManager _forkManager;
        private readonly IMetricsCollector _metricsCollector;

        public TableController(IForkManager forkManager, IMetricsCollector metricsCollector)
        {
            _forkManager = forkManager;
            _metricsCollector = metricsCollector;
        }

        [HttpPost("forks/{forkId:int}/take")]
        public IActionResult TakeFork(int forkId, [FromBody] TakeForkRequest request)
        {
            if (string.IsNullOrEmpty(request.PhilosopherId))
            {
                return BadRequest("PhilosopherId is required.");
            }

            if (_forkManager.TryTakeFork(forkId, request.PhilosopherId))
            {
                _metricsCollector.RecordForkUsage(forkId, true);
                return Ok();
            }
            
            return Conflict();
        }

        [HttpPost("forks/{forkId:int}/release")]
        public IActionResult ReleaseFork(int forkId)
        {
            _forkManager.ReleaseFork(forkId);
            _metricsCollector.RecordForkUsage(forkId, false);
            return Ok();
        }

        [HttpPost("metrics/statechange")]
        public IActionResult RecordStateChange([FromBody] StateChangeRequest request)
        {
            if (string.IsNullOrEmpty(request.PhilosopherId) || string.IsNullOrEmpty(request.NewState))
            {
                return BadRequest("PhilosopherId and NewState are required.");
            }
            _metricsCollector.RecordPhilosopherStateChange(request.PhilosopherId, request.NewState);
            return Ok();
        }

        [HttpPost("metrics/finish")]
        public IActionResult PhilosopherFinished([FromBody] TakeForkRequest request)
        {
            if (string.IsNullOrEmpty(request.PhilosopherId))
            {
                return BadRequest("PhilosopherId is required.");
            }
            _metricsCollector.RecordPhilosopherFinished(request.PhilosopherId);
            return Ok();
        }
    }
}

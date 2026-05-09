using Microsoft.AspNetCore.Mvc;
using SupportService.DTOs.AssignmentDTOs;
using SupportService.Services.Interfaces;

namespace SupportService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentsController(IAssignmentService assignmentService)
            => _assignmentService = assignmentService;

        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] AssignTicketRequest request)
        {
            try
            {
                var assignment = await _assignmentService.AssignTicketAsync(request);
                return Ok(assignment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("ticket/{ticketId:int}")]
        public async Task<IActionResult> GetByTicket(int ticketId)
        {
            var assignment = await _assignmentService.GetActiveAssignmentAsync(ticketId);
            return assignment is null
                ? NotFound(new { message = $"No active assignment for ticket #{ticketId}." })
                : Ok(assignment);
        }

        [HttpGet("agent/{agentId:int}")]
        public async Task<IActionResult> GetByAgent(int agentId)
            => Ok(await _assignmentService.GetAgentAssignmentsAsync(agentId));

        [HttpPost("resolve")]
        public async Task<IActionResult> Resolve([FromBody] ResolveAssignmentRequest request)
        {
            var result = await _assignmentService.ResolveAsync(request);
            return result
                ? Ok(new { message = $"Ticket #{request.TicketId} resolved successfully." })
                : NotFound(new { message = $"No active assignment for ticket #{request.TicketId}." });
        }
    }
}

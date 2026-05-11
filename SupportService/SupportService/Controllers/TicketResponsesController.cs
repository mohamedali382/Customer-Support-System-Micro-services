using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportService.DTOs;
using SupportService.Services.Interfaces;

namespace SupportService.Controllers
{
    [ApiController]
    [Route("api/ticket-responses")]
    [Authorize]
    public class TicketResponsesController : ControllerBase
    {
        private readonly ITicketResponseService _responseService;

        public TicketResponsesController(ITicketResponseService responseService)
            => _responseService = responseService;

        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> AddResponse([FromBody] AddTicketResponseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Content is required." });

            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var agentName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(agentId))
                return Unauthorized();

            try
            {
                request.AgentId = agentId;
                var response = await _responseService.AddResponseAsync(
                    request
                );

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("ticket/{ticketId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByTicket(int ticketId)
        {
            var responses = await _responseService.GetResponsesByTicketAsync(ticketId);
            return Ok(responses);
        }
    }
}
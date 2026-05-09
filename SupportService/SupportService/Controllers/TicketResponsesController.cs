using Microsoft.AspNetCore.Mvc;
using SupportService.DTOs;
using SupportService.Services.Interfaces;

namespace SupportService.Controllers
{
    [ApiController]
    [Route("api/ticket-responses")]
    public class TicketResponsesController : ControllerBase
    {
        private readonly ITicketResponseService _responseService;

        public TicketResponsesController(ITicketResponseService responseService)
            => _responseService = responseService;

        /// <summary>Add an agent response to a ticket</summary>
        [HttpPost]
        public async Task<IActionResult> AddResponse([FromBody] AddTicketResponseRequest request)
        {
            if (string.IsNullOrEmpty(request.Content))
                return BadRequest(new { message = "Content is required." });

            try
            {
                var response = await _responseService.AddResponseAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Get all responses for a ticket</summary>
        [HttpGet("ticket/{ticketId:int}")]
        public async Task<IActionResult> GetByTicket(int ticketId)
            => Ok(await _responseService.GetResponsesByTicketAsync(ticketId));
    }
}

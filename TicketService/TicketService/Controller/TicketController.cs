using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketService.DTOs;
using TicketService.Services;
using TicketService.Services.Interfaces;

namespace TicketService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IticketService _ticketService;
        public TicketController(IticketService ticketService)
        {
            _ticketService = ticketService;
        }
        [Authorize(Roles = "User")]
        [HttpPost("CreateTicket")]
        public async Task<IActionResult> CreateTicket([FromBody] TicketDto ticketDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var createdTicket = await _ticketService.CreateTicketAsync(userId, ticketDto);
            return Ok(createdTicket);
        }
        [AllowAnonymous]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var result = await _ticketService.UpdateTicketStatusAsync(id, dto.Status);
            if (!result)
                return NotFound();
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketDto dto)
        {
            var result = await _ticketService.AssignTicketAsync(id, dto.AgentId, dto.AgentName, dto.Priority);
            if (!result)
                return NotFound();
            return Ok();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
        var userId  = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var ticket = await _ticketService.GetTicketByIdAsync(id);
        if (ticket == null)
            return NotFound();


        if (!isAdmin && ticket.UserId != userId)
            return Forbid();

            return Ok(ticket);
        }

        [Authorize]
        [HttpGet("mytickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var tickets = await _ticketService.GetAllTicketsAsync(userId);
            return Ok(tickets);
        }

        [AllowAnonymous]  
        [HttpGet("resolved")]
        public async Task<IActionResult> GetResolved(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
        {
            var tickets = await _ticketService.GetResolvedTicketsAsync(from, to);
            return Ok(tickets);
        }
    }
}

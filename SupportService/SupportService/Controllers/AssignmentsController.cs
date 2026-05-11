using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportService.DTOs.AssignmentDTOs;
using SupportService.Services.Interfaces;

namespace SupportService.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
        => _assignmentService = assignmentService;

    [HttpGet("open")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _assignmentService.GetAllAsync();
        return Ok(tickets);
    }

    // =========================
    // GET: api/tickets/open/{id}
    // (Only OPEN ticket by ID)
    // =========================
    [HttpGet("open/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOpenById(int id)
    {
        var ticket = await _assignmentService.GetOpenByIdAsync(id);

        if (ticket is null)
        {
            return NotFound(new
            {
                message = $"Open ticket #{id} not found."
            });
        }

        return Ok(ticket);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign([FromBody] AssignTicketRequest request)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        request.AssignedBy = adminName ?? adminId;

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
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> GetByTicket(int ticketId)
    {
        var assignment = await _assignmentService.GetActiveAssignmentAsync(ticketId);
        return assignment is null
            ? NotFound(new { message = $"No active assignment for ticket #{ticketId}." })
            : Ok(assignment);
    }


    [HttpGet("agent/{agentId}")]
    [Authorize]
    public async Task<IActionResult> GetByAgent(string agentId)
    {
        if (User.IsInRole("Agent"))
        {
            var currentAgentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentAgentId != agentId)
                return Forbid();

            Console.WriteLine("user is agent");
        }

        var assignments = await _assignmentService.GetAgentAssignmentsAsync(agentId);
        return Ok(assignments);
    }


    [HttpPost("resolve")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Resolve([FromBody] ResolveAssignmentRequest request)
    {
        var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(agentId))
            return Unauthorized();

        var assignment = await _assignmentService.GetActiveAssignmentAsync(request.TicketId);
        if (assignment is null)
            return NotFound(new { message = $"No active assignment for ticket #{request.TicketId}." });

        if (assignment.AgentId != agentId)
            return Forbid();

        request.AgentId = agentId;

        var result = await _assignmentService.ResolveAsync(request);
        return result
            ? Ok(new { message = $"Ticket #{request.TicketId} resolved successfully." })
            : NotFound(new { message = $"No active assignment for ticket #{request.TicketId}." });
    }
}
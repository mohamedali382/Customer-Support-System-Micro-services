using Microsoft.AspNetCore.Mvc;
using SupportService.DTOs.AgentDTOs;
using SupportService.Services.Interfaces;

namespace SupportService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentsController(IAgentService agentService) => _agentService = agentService;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null)
            => Ok(await _agentService.GetAllAsync(status));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var agent = await _agentService.GetByIdAsync(id);
            return agent is null ? NotFound(new { message = $"Agent #{id} not found." }) : Ok(agent);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAgentRequest request)
        {
            if (string.IsNullOrEmpty(request.Name)) return BadRequest(new { message = "Name is required." });
            if (string.IsNullOrEmpty(request.Email)) return BadRequest(new { message = "Email is required." });

            try
            {
                var agent = await _agentService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = agent.Id }, agent);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAgentRequest request)
        {
            var agent = await _agentService.UpdateAsync(id, request);
            return agent is null ? NotFound(new { message = $"Agent #{id} not found." }) : Ok(agent);
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var valid = new[] { "Available", "Busy", "Offline" };
            if (!valid.Contains(request.Status))
                return BadRequest(new { message = $"Status must be one of: {string.Join(", ", valid)}" });

            var agent = await _agentService.UpdateAsync(id,
                new UpdateAgentRequest(null, null, null, null, request.Status));

            return agent is null ? NotFound(new { message = $"Agent #{id} not found." }) : Ok(agent);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _agentService.DeleteAsync(id);
            return result ? NoContent() : NotFound(new { message = $"Agent #{id} not found." });
        }
    }
}

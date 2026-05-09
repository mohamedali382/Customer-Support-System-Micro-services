using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketService.DTOs;
using TicketService.Data;
using Microsoft.EntityFrameworkCore;
using TicketService.Services.Interfaces;
using Contracts;
namespace TicketService.Services
{
    public class TicketService : IticketService
    {
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _publisher;

        public TicketService(AppDbContext context, RabbitMQPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }
        public async Task<TicketDto> CreateTicketAsync(string userId, TicketDto ticketDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID cannot be null or empty.");
                if (ticketDto == null) throw new ArgumentNullException(nameof(ticketDto));
                if (string.IsNullOrEmpty(ticketDto.Title)) throw new ArgumentException("Title cannot be null or empty.");
                if (string.IsNullOrEmpty(ticketDto.Description)) throw new ArgumentException("Description cannot be null or empty.");

                var ticket = new Entities.Ticket
                {
                    Title = ticketDto.Title,
                    Description = ticketDto.Description,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    Status = "open",
                    UserId = userId
                };
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                await _publisher.PublishTicketCreatedAsync(new TicketCreatedEvent
                {
                    TicketId = ticket.Id,
                    UserId = userId,
                    Title = ticket.Title,
                    Status = ticket.Status,
                    Description = ticket.Description,
                    CreatedAt = ticket.Created
                });

                return new TicketDto
                {
                    Title = ticket.Title,
                    Description = ticket.Description
                };
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new ApplicationException("An error occurred while creating the ticket.", ex);
            }
            
        }

        public async Task<TicketDtoResponse?> GetTicketByIdAsync(int id, string userId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t =>t.Id == id && t.UserId == userId);

            if (ticket == null)
                return null;

            return new TicketDtoResponse
            {
                Id = ticket.Id.ToString(),
                Title = ticket.Title,
                Description = ticket.Description,
                Created = ticket.Created,
                Updated = ticket.Updated,
                Status = ticket.Status
            };
        }

        public async Task<bool> UpdateTicketStatusAsync(int id, string status)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return false;

            ticket.Status = status;
            ticket.Updated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignTicketAsync(int ticketId, int agentId, string agentName, string priority)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return false;

            ticket.AssignedAgentId = agentId;
            ticket.AssignedAgentName = agentName;
            ticket.Priority = priority;

            ticket.Status = "Assigned";
            ticket.Updated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<TicketDtoResponse>> GetAllTicketsAsync(string userId)
        {
            var tickets = await _context.Tickets.Where(t => t.UserId == userId).ToListAsync();
            return tickets.Select(ticket => new TicketDtoResponse
            {
                Id = ticket.Id.ToString(),
                Title = ticket.Title,
                Description = ticket.Description,
                Created = ticket.Created,
                Updated = ticket.Updated,
                Status = ticket.Status
            }).ToList();
        }

    }
}

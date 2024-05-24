


using EfExample.Context;
using EfExample.DTOs;
using EfExample.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EfExample.Controllers;

[Route("api/")]
[ApiController]
public class TripController : ControllerBase
{
    private readonly ApbdContext _context;
    public TripController(ApbdContext context)
    {
        _context = context;
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetTripsAsync()
    {
        var trips = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.CountryTrips)
            .ThenInclude(ct => ct.IdCountryNavigation)
            .Select(t => new TripsDTO
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.CountryTrips.Select(ct => new CountryDTO()
                {
                    Name = ct.IdCountryNavigation.Name
                }),
                Clients = t.ClientTrips.Select(ct => new ClientDTO()
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                })
            })
            .ToListAsync();
        return Ok(trips);
    }

    [HttpPost("trips/{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTripAsync(int idTrip, AddClientDTO addClientDto)
    {
        var client = await _context.Clients
            .SingleOrDefaultAsync(c => c.Pesel == addClientDto.Pesel);

        if (client is null)
        {
            var maxId = await _context.Clients.MaxAsync(c => c.IdClient);
            var newClient = new Client()
            {
                IdClient = maxId + 1,
                FirstName = addClientDto.FirstName,
                LastName = addClientDto.LastName,
                Email = addClientDto.Email,
                Pesel = addClientDto.Pesel,
                Telephone = addClientDto.Telephone
            };
            await _context.Clients.AddAsync(newClient);
            await _context.SaveChangesAsync();
            client = await _context.Clients
                .SingleOrDefaultAsync(c => c.Pesel == addClientDto.Pesel);
        }
        
        var trip = await _context.Trips.FindAsync(idTrip);
        if (trip is null) return NotFound("Trip not found");
        
        var clientTrip = await _context.ClientTrips
            .FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip);
        if (clientTrip is not null) return Conflict("Client is already assigned to this trip");
        
        var newClientTrip = new ClientTrip()
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = addClientDto.PaymentDate
        };
        
        await _context.ClientTrips.AddAsync(newClientTrip);
        await _context.SaveChangesAsync();
    
        return NoContent();
    }
    
    
    [HttpDelete("clients/{idClient}")]
    public async Task<IActionResult> DeleteClientAsync(int idClient)
    {
        var client = await _context.Clients.FindAsync(idClient);
        if (client is null) return NotFound("Client not found");
        
        var clientTrip = await _context.ClientTrips
            .FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient);
        if (clientTrip is not null) return Conflict("Client has assigned trips");
        
        await _context.Clients
            .Where(s => s.IdClient == idClient)
            .ExecuteDeleteAsync();
        return Ok();
    }

}
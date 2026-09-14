using Levelbuild.CodingChallenge.Api.Dtos;
using Levelbuild.CodingChallenge.Data;
using Levelbuild.CodingChallenge.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace Levelbuild.CodingChallenge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomerController : Controller
{
    private readonly CodingChallengeDbContext _db;

    public CustomerController(CodingChallengeDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [EnableQuery]
    public IActionResult List()
    {
        return Ok(_db.Customers.AsNoTracking());
    }

    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (await _db.Customers.AnyAsync(c => c.Name == request.Name))
        {
            return Conflict(new ProblemDetails
            {
                Title = "A customer with this name already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            WebSite = request.WebSite
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
    }

    [HttpPatch]
    [Route("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                ModelState.AddModelError(nameof(request.Name), "Name cannot be empty.");
                return ValidationProblem(ModelState);
            }

            if (request.Name != customer.Name && await _db.Customers.AnyAsync(c => c.Name == request.Name))
            {
                return Conflict(new ProblemDetails
                {
                    Title = "A customer with this name already exists.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            customer.Name = request.Name;
        }

        if (request.WebSite is not null)
        {
            customer.WebSite = request.WebSite;
        }

        await _db.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

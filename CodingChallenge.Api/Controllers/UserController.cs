using Levelbuild.CodingChallenge.Api.Dtos;
using Levelbuild.CodingChallenge.Data;
using Levelbuild.CodingChallenge.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace Levelbuild.CodingChallenge.Api.Controllers;

[ApiController]
[Route("api/Customer/{customerId:guid}/[controller]")]
[Produces("application/json")]
public class UserController : Controller
{
    private readonly CodingChallengeDbContext _db;

    public UserController(CodingChallengeDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> List(Guid customerId)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == customerId))
        {
            return NotFound();
        }

        return Ok(_db.Users.AsNoTracking().Where(u => u.CustomerId == customerId));
    }

    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> Get(Guid customerId, Guid id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.CustomerId == customerId && u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid customerId, [FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!await _db.Customers.AnyAsync(c => c.Id == customerId))
        {
            return NotFound();
        }

        if (await _db.Users.AnyAsync(u => u.DisplayName == request.DisplayName))
        {
            return Conflict(new ProblemDetails
            {
                Title = "A user with this display name already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict(new ProblemDetails
            {
                Title = "A user with this email already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            DisplayName = request.DisplayName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { customerId, id = user.Id }, user);
    }

    [HttpPatch]
    [Route("{id:guid}")]
    public async Task<IActionResult> Update(Guid customerId, Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.CustomerId == customerId && u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        if (request.DisplayName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                ModelState.AddModelError(nameof(request.DisplayName), "DisplayName cannot be empty.");
                return ValidationProblem(ModelState);
            }

            if (request.DisplayName != user.DisplayName &&
                await _db.Users.AnyAsync(u => u.DisplayName == request.DisplayName))
            {
                return Conflict(new ProblemDetails
                {
                    Title = "A user with this display name already exists.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            user.DisplayName = request.DisplayName;
        }

        if (request.Email is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                ModelState.AddModelError(nameof(request.Email), "Email cannot be empty.");
                return ValidationProblem(ModelState);
            }

            if (request.Email != user.Email && await _db.Users.AnyAsync(u => u.Email == request.Email))
            {
                return Conflict(new ProblemDetails
                {
                    Title = "A user with this email already exists.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            user.Email = request.Email;
        }

        if (request.FirstName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                ModelState.AddModelError(nameof(request.FirstName), "FirstName cannot be empty.");
                return ValidationProblem(ModelState);
            }

            user.FirstName = request.FirstName;
        }

        if (request.LastName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                ModelState.AddModelError(nameof(request.LastName), "LastName cannot be empty.");
                return ValidationProblem(ModelState);
            }

            user.LastName = request.LastName;
        }

        if (request.DateOfBirth is not null)
        {
            user.DateOfBirth = request.DateOfBirth.Value;
        }

        await _db.SaveChangesAsync();

        return Ok(user);
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public async Task<IActionResult> Delete(Guid customerId, Guid id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.CustomerId == customerId && u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

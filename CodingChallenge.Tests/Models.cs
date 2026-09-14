namespace CodingChallenge.Tests;

public record CustomerModel(Guid Id, string Name, string? WebSite);

public record UserModel(
    Guid Id,
    Guid CustomerId,
    string DisplayName,
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth);

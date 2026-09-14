using System.Text.Json.Serialization;

namespace Levelbuild.CodingChallenge.Data.Entities;

public class User
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    [JsonIgnore]
    public Customer? Customer { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
}

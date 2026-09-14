using System.Text.Json.Serialization;

namespace Levelbuild.CodingChallenge.Data.Entities;

public class Customer
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? WebSite { get; set; }

    [JsonIgnore]
    public List<User> Users { get; set; } = new();
}

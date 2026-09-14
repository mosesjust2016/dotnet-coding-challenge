using System.ComponentModel.DataAnnotations;

namespace Levelbuild.CodingChallenge.Api.Dtos;

public class CreateCustomerRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? WebSite { get; set; }
}

public class UpdateCustomerRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? WebSite { get; set; }
}

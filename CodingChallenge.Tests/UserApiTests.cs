using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace CodingChallenge.Tests;

public class UserApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_User_ReturnsCustomerIdOnly()
    {
        var customer = await CreateCustomer();

        var response = await _client.PostAsJsonAsync($"/api/customer/{customer.Id}/user", new
        {
            DisplayName = $"user-{Guid.NewGuid()}",
            FirstName = "Jane",
            LastName = "Doe",
            Email = $"{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1985, 5, 5)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("\"customer\":", "user response objects must only contain the associated customer's Id");

        var user = await response.Content.ReadFromJsonAsync<UserModel>();
        user!.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public async Task Create_User_InvalidEmail_ReturnsBadRequest()
    {
        var customer = await CreateCustomer();

        var response = await _client.PostAsJsonAsync($"/api/customer/{customer.Id}/user", new
        {
            DisplayName = $"user-{Guid.NewGuid()}",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "not-an-email",
            DateOfBirth = new DateTime(1985, 5, 5)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_User_ForNonExistentCustomer_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync($"/api/customer/{Guid.NewGuid()}/user", new
        {
            DisplayName = $"user-{Guid.NewGuid()}",
            FirstName = "Jane",
            LastName = "Doe",
            Email = $"{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1985, 5, 5)
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_User_DuplicateEmail_ReturnsConflict()
    {
        var customer = await CreateCustomer();
        var email = $"{Guid.NewGuid()}@example.com";

        var first = await _client.PostAsJsonAsync($"/api/customer/{customer.Id}/user", new
        {
            DisplayName = $"user-{Guid.NewGuid()}",
            FirstName = "Jane",
            LastName = "Doe",
            Email = email,
            DateOfBirth = new DateTime(1985, 5, 5)
        });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync($"/api/customer/{customer.Id}/user", new
        {
            DisplayName = $"user-{Guid.NewGuid()}",
            FirstName = "John",
            LastName = "Smith",
            Email = email,
            DateOfBirth = new DateTime(1990, 1, 1)
        });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_User_RemovesUserWithoutAffectingCustomer()
    {
        var customer = await CreateCustomer();
        var userResponse = await _client.PostAsJsonAsync($"/api/customer/{customer.Id}/user", new
        {
            DisplayName = $"user-{Guid.NewGuid()}",
            FirstName = "Jane",
            LastName = "Doe",
            Email = $"{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1985, 5, 5)
        });
        var user = await userResponse.Content.ReadFromJsonAsync<UserModel>();

        var deleteResponse = await _client.DeleteAsync($"/api/customer/{customer.Id}/user/{user!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getCustomer = await _client.GetAsync($"/api/customer/{customer.Id}");
        getCustomer.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<CustomerModel> CreateCustomer()
    {
        var response = await _client.PostAsJsonAsync("/api/customer", new { Name = $"Customer-{Guid.NewGuid()}" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerModel>())!;
    }
}

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace CodingChallenge.Tests;

public class CustomerApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomerApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Then_Get_ReturnsCustomer()
    {
        var response = await _client.PostAsJsonAsync("/api/customer", new { Name = $"Acme-{Guid.NewGuid()}", WebSite = "https://acme.example" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CustomerModel>();
        created.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/customer/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<CustomerModel>();
        fetched!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Create_MissingName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/customer", new { WebSite = "https://no-name.example" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        var name = $"Duplicate-{Guid.NewGuid()}";
        var first = await _client.PostAsJsonAsync("/api/customer", new { Name = name });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/api/customer", new { Name = name });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Get_NonExistentCustomer_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/customer/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_PartialFields_OnlyUpdatesProvidedFields()
    {
        var created = await CreateCustomer();

        var patchResponse = await _client.PatchAsJsonAsync($"/api/customer/{created.Id}", new { WebSite = "https://updated.example" });
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await patchResponse.Content.ReadFromJsonAsync<CustomerModel>();
        updated!.Name.Should().Be(created.Name);
        updated.WebSite.Should().Be("https://updated.example");
    }

    [Fact]
    public async Task Delete_Customer_CascadesToUsers()
    {
        var customer = await CreateCustomer();

        var userResponse = await _client.PostAsJsonAsync($"/api/customer/{customer.Id}/user", new
        {
            DisplayName = $"user-{Guid.NewGuid()}",
            FirstName = "John",
            LastName = "Doe",
            Email = $"{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        });
        userResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await userResponse.Content.ReadFromJsonAsync<UserModel>();

        var deleteResponse = await _client.DeleteAsync($"/api/customer/{customer.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getCustomer = await _client.GetAsync($"/api/customer/{customer.Id}");
        getCustomer.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var getUser = await _client.GetAsync($"/api/customer/{customer.Id}/user/{user!.Id}");
        getUser.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_ReturnsArray_WithoutUsersField()
    {
        await CreateCustomer();

        var response = await _client.GetAsync("/api/customer");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("\"users\"", "customer response objects must not contain associated users");
        body.Should().NotContain("\"Users\"");
    }

    private async Task<CustomerModel> CreateCustomer()
    {
        var response = await _client.PostAsJsonAsync("/api/customer", new { Name = $"Customer-{Guid.NewGuid()}", WebSite = "https://example.com" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerModel>())!;
    }
}

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace CodingChallenge.Tests;

public class ODataQueryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ODataQueryTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Filter_ReturnsOnlyMatchingCustomers()
    {
        var uniqueTag = Guid.NewGuid().ToString("N");
        await CreateCustomer($"Filterable-{uniqueTag}");
        await CreateCustomer($"Other-{Guid.NewGuid()}");

        var response = await _client.GetAsync($"/api/customer?$filter=contains(Name,'{uniqueTag}')");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<CustomerModel>>();
        results.Should().OnlyContain(c => c.Name.Contains(uniqueTag));
    }

    [Fact]
    public async Task Select_ReturnsOnlyRequestedFields()
    {
        await CreateCustomer($"Selectable-{Guid.NewGuid()}");

        var response = await _client.GetAsync("/api/customer?$select=Name&$top=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("\"Id\"");
        body.Should().Contain("\"Name\"");
    }

    [Fact]
    public async Task TopAndSkip_PageResults()
    {
        var tag = Guid.NewGuid().ToString("N");
        for (var i = 0; i < 5; i++)
        {
            await CreateCustomer($"Page-{tag}-{i}");
        }

        var response = await _client.GetAsync($"/api/customer?$filter=contains(Name,'{tag}')&$orderby=Name&$top=2&$skip=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<CustomerModel>>();
        results.Should().HaveCount(2);
        results![0].Name.Should().Be($"Page-{tag}-1");
        results[1].Name.Should().Be($"Page-{tag}-2");
    }

    private async Task<CustomerModel> CreateCustomer(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/customer", new { Name = name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerModel>())!;
    }
}

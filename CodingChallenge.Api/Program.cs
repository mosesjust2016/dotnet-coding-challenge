using Levelbuild.CodingChallenge.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddOData(options => options.Select().Filter().OrderBy().SetMaxTop(1000).Count());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=codingchallenge.db";

builder.Services.AddDbContext<CodingChallengeDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CodingChallengeDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;

        var (status, title) = exception switch
        {
            DbUpdateException => (StatusCodes.Status409Conflict, "The request could not be completed due to a data conflict."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.MapControllers();

app.Run();

public partial class Program;

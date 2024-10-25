using ElasticSearch.Api;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Client>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/add", async (Client client, [FromBody] Documentation documentation, CancellationToken token) =>
{
    await client.Add(documentation, token);
});
app.MapGet(
    "/api/all",
    async (Client client, CancellationToken token) =>
        Results.Ok(await client.All(token)
    )
);
app.MapGet(
    "/api/search-with-fuzzy",
    async (Client client, [FromQuery] string text, [FromQuery] string field, CancellationToken token, [FromQuery] int? @from = default, [FromQuery] int? size = default) =>
        Results.Ok(await client.SearchWithFuzzy(text, field, token, @from, size)
    )
);
app.MapGet(
    "/api/search-with-match",
    async (Client client,[FromQuery] string text, [FromQuery] string field, CancellationToken token, [FromQuery] int? @from = default, [FromQuery] int? size = default) =>
        Results.Ok(await client.SearchWithMatch(text, field, token, @from, size)
    )
);
app.MapGet(
    "/api/search-with-wildcard",
    async (Client client,[FromQuery] string text, [FromQuery] string field, CancellationToken token, [FromQuery] int? @from = default, [FromQuery] int? size = default) =>
        Results.Ok(await client.SearchWithWildcard(text, field, token, @from, size)
    )
);

app.Run();

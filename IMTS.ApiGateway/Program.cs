using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/notifications", async (HttpClient httpClient, [AsParameters] NotificationQuery query) =>
{
    var url = $"http://localhost:5242/api/notifications?userId={query.UserId}&filter={query.Filter}&page={query.Page}&pageSize={query.PageSize}";
    return await httpClient.GetFromJsonAsync<object>(url);
});

app.MapGet("/api/notifications/unread-count", async (HttpClient httpClient, [AsParameters] NotificationQuery query) =>
{
    var url = $"http://localhost:5242/api/notifications/unread-count?userId={query.UserId}";
    return await httpClient.GetFromJsonAsync<object>(url);
});

app.MapPost("/api/notifications", async (HttpClient httpClient, object payload) =>
{
    var response = await httpClient.PostAsJsonAsync("http://localhost:5242/api/notifications", payload);
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapPut("/api/notifications/{id}/read", async (HttpClient httpClient, int id) =>
{
    var response = await httpClient.PutAsync($"http://localhost:5242/api/notifications/{id}/read", null);
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapPut("/api/notifications/read-all", async (HttpClient httpClient, [AsParameters] NotificationQuery query) =>
{
    var response = await httpClient.PutAsync($"http://localhost:5242/api/notifications/read-all?userId={query.UserId}", null);
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapDelete("/api/notifications/{id}", async (HttpClient httpClient, int id) =>
{
    var response = await httpClient.DeleteAsync($"http://localhost:5242/api/notifications/{id}");
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapGet("/api/resources", async (HttpClient httpClient, [AsParameters] ResourceQuery query) =>
{
    var url = $"http://localhost:5201/api/resources?category={query.Category}&search={query.Search}&page={query.Page}&pageSize={query.PageSize}";
    return await httpClient.GetFromJsonAsync<object>(url);
});

app.MapGet("/api/resources/stats", async (HttpClient httpClient) =>
{
    return await httpClient.GetFromJsonAsync<object>("http://localhost:5201/api/resources/stats");
});

app.MapPost("/api/resources", async (HttpClient httpClient, object payload) =>
{
    var response = await httpClient.PostAsJsonAsync("http://localhost:5201/api/resources", payload);
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapPut("/api/resources/{id}", async (HttpClient httpClient, int id, object payload) =>
{
    var response = await httpClient.PutAsJsonAsync($"http://localhost:5201/api/resources/{id}", payload);
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapDelete("/api/resources/{id}", async (HttpClient httpClient, int id) =>
{
    var response = await httpClient.DeleteAsync($"http://localhost:5201/api/resources/{id}");
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapGet("/api/resources/download/{id}", async (HttpClient httpClient, int id) =>
{
    return await httpClient.GetFromJsonAsync<object>($"http://localhost:5201/api/resources/download/{id}");
});

app.MapPost("/api/email/send", async (HttpClient httpClient, object payload) =>
{
    var response = await httpClient.PostAsJsonAsync("http://localhost:5211/api/email/send", payload);
    return Results.Json(await response.Content.ReadFromJsonAsync<object>());
});

app.MapGet("/api/email/queue", async (HttpClient httpClient) =>
{
    return await httpClient.GetFromJsonAsync<object>("http://localhost:5211/api/email/queue");
});

app.Run();

public sealed record NotificationQuery(string? UserId, string? Filter, int Page = 1, int PageSize = 20);
public sealed record ResourceQuery(string? Category, string? Search, int Page = 1, int PageSize = 12);

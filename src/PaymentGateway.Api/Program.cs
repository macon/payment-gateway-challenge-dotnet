using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api;
using PaymentGateway.Api.Infra;
using PaymentGateway.App.Infra;
using PaymentGateway.App.Logging;
using PaymentGateway.App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IBankClient, BankClient>(httpClient =>
{
    // TODO: Would also add some form of retry policy and there are all sorts of approaches.
    httpClient.BaseAddress = new Uri("http://localhost:8080/");
    httpClient.Timeout = TimeSpan.FromSeconds(5);
});

// TODO: document use of Decorator.
builder.Services.Decorate<IBankClient, BankClientLogger>();

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<JsonOptions>(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

// TODO: Turned off model state validation middleware so all validation can be handled in same place consistently.
builder.Services.Configure<ApiBehaviorOptions>(opts => opts.SuppressModelStateInvalidFilter = true);
builder.Services.AddScoped<PaymentService, PaymentService>();

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddSingleton<PaymentRequestCache>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

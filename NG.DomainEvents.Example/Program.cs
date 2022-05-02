using NG.DomainEvents.Example;
using NG.DomainEvents.Example.Handlers;
using NG.DomainEvents.Example.Models.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NG.DomainEvents.Config;
using NG.DomainEvents.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TestDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddDomainEvents<TestDbContext>(builder.Configuration, typeof(TestHandler1).Assembly);
builder.Services.Configure<DomainEventsConfig>(builder.Configuration.GetSection(""));

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
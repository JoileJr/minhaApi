using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;
using MinhaApi.Middleware;
using MinhaApi.Services;
using MinhaApi.Settings;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IHelloService, HelloService>();
builder.Services.AddScoped<IObjetoService, ObjetoService>();

builder.Services.Configure<ViaCepSettings>(
    builder.Configuration.GetSection(ViaCepSettings.SectionName));

builder.Services.AddHttpClient<IViaCepService, ViaCepService>(client =>
{
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MinhaApi/1.0");
}).AddStandardResilienceHandler();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

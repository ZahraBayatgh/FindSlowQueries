using FindSlowQueries.Data;
using FindSlowQueries.Interceptors;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(provider.GetRequiredService<QueryInterceptor>());
});

builder.Services.AddSingleton<QueryInterceptor>();

var logger = new LoggerConfiguration()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        MinimumLogEventLevel = Serilog.Events.LogEventLevel.Warning,
        IndexFormat = "logs-dotnet-{0:yyyy.MM}"

    })
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
          {
              AutoRegisterTemplate = true,
              MinimumLogEventLevel = Serilog.Events.LogEventLevel.Warning
          });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

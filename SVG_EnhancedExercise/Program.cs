//
// Entry point for the application
var builder = WebApplication.CreateBuilder(args);

// Configure logging
// Programmer can use Serilog, NLog, etc. for logging to the files too. For this task I used default logging providers. To enable 
// logging to the files, add the Serilog.Extensions.logging or Serilog.Sinks.File NuGet packages and configure the logging providers.
builder.Logging.ClearProviders();  // Clear default providers
builder.Logging.AddConsole();      // Add Console logging
builder.Logging.AddDebug();        // Add Debug logging

// Add services to the dependency injection (DI) container
builder.Services.AddControllers();     // Add MVC services to the DI container
builder.Services.AddEndpointsApiExplorer(); // Enable OpenAPI for endpoint exploration
builder.Services.AddSwaggerGen();    // Enable Swagger for API documentation

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // Enable Swagger UI in development
    app.UseSwaggerUI();    // Serve Swagger UI for testing APIs
}

app.UseHttpsRedirection();   // Redirect HTTP requests to HTTPS
app.UseAuthorization();      // Enable authorization middleware

// Map controller routes to the API
app.MapControllers();

app.Run();

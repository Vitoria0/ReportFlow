using Vendas.Application;
using Vendas.Infrastructure;
using Vendas.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<VendasDbContext>();
	dbContext.Database.EnsureCreated();
}

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendas API v1");
	options.RoutePrefix = "swagger";
});

app.MapControllers();

app.Run();

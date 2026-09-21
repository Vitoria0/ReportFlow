using Microsoft.EntityFrameworkCore;
using Vendas.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VendasDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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

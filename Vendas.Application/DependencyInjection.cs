using Microsoft.Extensions.DependencyInjection;
using Vendas.Application.UseCases.Relatorios;
using Vendas.Application.UseCases.Vendas;

namespace Vendas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegistrarVendaHandler>();
        services.AddScoped<ListarVendasHandler>();
        services.AddScoped<ObterVendaPorIdHandler>();
        services.AddScoped<SolicitarRelatorioHandler>();
        return services;
    }
}

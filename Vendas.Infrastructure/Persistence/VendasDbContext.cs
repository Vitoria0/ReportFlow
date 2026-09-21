using Microsoft.EntityFrameworkCore;
using Vendas.Application.Abstractions;
using Vendas.Domain.Entities;

namespace Vendas.Infrastructure.Persistence;

public sealed class VendasDbContext(DbContextOptions<VendasDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<ReportRequest> ReportRequests => Set<ReportRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venda>(entity =>
        {
            entity.ToTable("Vendas");
            entity.HasKey(venda => venda.Id);
            entity.Property(venda => venda.Cliente).HasMaxLength(200).IsRequired();
            entity.Property(venda => venda.ValorTotal).HasPrecision(18, 2);
            entity.Property(venda => venda.DataVenda).IsRequired();
            entity.Property(venda => venda.CreatedAt).IsRequired();
            entity.HasMany(venda => venda.Itens)
                .WithOne(item => item.Venda)
                .HasForeignKey(item => item.VendaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemVenda>(entity =>
        {
            entity.ToTable("ItensVenda");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Produto).HasMaxLength(200).IsRequired();
            entity.Property(item => item.ValorUnitario).HasPrecision(18, 2);
            entity.Property(item => item.ValorTotal).HasPrecision(18, 2);
            entity.Property(item => item.Quantidade).IsRequired();
        });

        modelBuilder.Entity<ReportRequest>(entity =>
        {
            entity.ToTable("ReportRequests");
            entity.HasKey(request => request.Id);
            entity.Property(request => request.StartDate).IsRequired();
            entity.Property(request => request.EndDate).IsRequired();
            entity.Property(request => request.Status).HasMaxLength(20).IsRequired();
            entity.Property(request => request.CreatedAt).IsRequired();
            entity.Property(request => request.ErrorMessage).HasMaxLength(2000);
        });
    }
}

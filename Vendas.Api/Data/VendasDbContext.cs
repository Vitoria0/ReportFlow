using Microsoft.EntityFrameworkCore;
using Vendas.Api.Models;

namespace Vendas.Api.Data;

public sealed class VendasDbContext(DbContextOptions<VendasDbContext> options) : DbContext(options)
{
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

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
    }
}

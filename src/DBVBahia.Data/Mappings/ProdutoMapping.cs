using DBVBahia.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBVBahia.Data.Mappings
{
    public class ProdutoMapping : IEntityTypeConfiguration<Produto>
    {
        public void Configure(EntityTypeBuilder<Produto> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasColumnType("varchar(200)");

            builder.Property(p => p.Descricao)
                .IsRequired()
                .HasColumnType("varchar(1000)");

            // 1 : 1 => Produto : Picture
            builder.HasOne(f => f.Picture)
                .WithOne(e => e.Produto)
                .OnDelete(DeleteBehavior.Cascade);

            // N : 1 => Fornecedor : Produtos
            builder.HasOne(f => f.Fornecedor)
                .WithMany(p => p.Produtos)
                .HasForeignKey(p => p.FornecedorId);

            builder.ToTable("Produtos");
        }
    }
}
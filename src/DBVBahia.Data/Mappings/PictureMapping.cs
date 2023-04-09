using DBVBahia.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBVBahia.Data.Mappings
{
    public class PictureMapping : IEntityTypeConfiguration<Picture>
    {
        public void Configure(EntityTypeBuilder<Picture> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasColumnType("varchar(100)");

            // 1 : 1 => Picture : Produto
            builder.HasOne(f => f.Produto)
                .WithOne(e => e.Picture);

            builder.Property(p => p.Image)
                .IsRequired()
                .HasColumnType("varbinary(max)");

            builder.ToTable("Pictures");
        }
    }
}
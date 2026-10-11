using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Players;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players", table => table.HasCheckConstraint(
            "CK_Players_ShirtNumber",
            $"[ShirtNumber] BETWEEN {Player.MinShirtNumber} AND {Player.MaxShirtNumber}"));

        builder.HasKey(player => player.Id);
        builder.Property(player => player.Id).ValueGeneratedNever();

        builder.Property(player => player.FullName).HasMaxLength(Player.FullNameMaxLength).IsRequired();

        // La posición se guarda como texto ("Forward") para que la BD sea legible.
        builder.Property(player => player.Position).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Respaldo en BD de la regla del dominio: número único dentro del equipo.
        builder.HasIndex(player => new { player.TeamId, player.ShirtNumber }).IsUnique();
    }
}

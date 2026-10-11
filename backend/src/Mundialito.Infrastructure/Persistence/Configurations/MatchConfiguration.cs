using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Teams;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches", table => table.HasCheckConstraint(
            "CK_Matches_DifferentTeams",
            "[HomeTeamId] <> [AwayTeamId]"));

        builder.HasKey(match => match.Id);
        builder.Property(match => match.Id).ValueGeneratedNever();

        builder.Property(match => match.Venue).HasMaxLength(Match.VenueMaxLength);
        builder.Property(match => match.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Restrict: un equipo con partidos no se puede borrar (y evita rutas de cascada múltiples).
        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(match => match.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(match => match.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(match => match.Goals)
            .WithOne()
            .HasForeignKey(goal => goal.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(match => match.Goals)
            .HasField("_goals")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Índices para los filtros por fecha y estado.
        builder.HasIndex(match => match.ScheduledAt);
        builder.HasIndex(match => match.Status);

        builder.Ignore(match => match.DomainEvents);
    }
}

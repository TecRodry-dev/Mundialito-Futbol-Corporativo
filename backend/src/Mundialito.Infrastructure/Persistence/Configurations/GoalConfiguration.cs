using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Players;
using Mundialito.Domain.Teams;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("Goals");

        builder.HasKey(goal => goal.Id);
        builder.Property(goal => goal.Id).ValueGeneratedNever();

        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(goal => goal.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(goal => goal.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para la consulta de goleadores.
        builder.HasIndex(goal => goal.PlayerId);
    }
}

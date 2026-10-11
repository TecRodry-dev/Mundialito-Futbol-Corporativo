using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Teams;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        builder.HasKey(team => team.Id);
        builder.Property(team => team.Id).ValueGeneratedNever();

        builder.Property(team => team.Name).HasMaxLength(Team.NameMaxLength).IsRequired();
        builder.HasIndex(team => team.Name).IsUnique();

        builder.Property(team => team.Coach).HasMaxLength(Team.CoachMaxLength).IsRequired();

        builder.HasMany(team => team.Players)
            .WithOne()
            .HasForeignKey(player => player.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(team => team.Players)
            .HasField("_players")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(team => team.DomainEvents);
    }
}

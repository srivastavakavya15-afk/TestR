using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestR.Domain.Users;

namespace TestR.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(User.NameMaxLength);
        builder.Property(x => x.Age).IsRequired();
        builder.Property(x => x.City).IsRequired().HasMaxLength(User.CityMaxLength);
        builder.Property(x => x.State).IsRequired().HasMaxLength(User.StateMaxLength);
        builder.Property(x => x.Pincode).IsRequired().HasMaxLength(User.PincodeMaxLength);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasConversion(
                utc => utc,
                stored => DateTime.SpecifyKind(stored, DateTimeKind.Utc));

        builder.HasIndex(x => x.CreatedAtUtc);
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IMTS.Shared.Models;

#nullable disable

namespace IMTS.NotificationService.Migrations
{
    [DbContext(typeof(NotificationDbContext))]
    partial class NotificationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0");

            modelBuilder.Entity("IMTS.Shared.Models.Notification", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("ActionUrl")
                    .HasColumnType("nvarchar(500)")
                    .HasMaxLength(500);

                b.Property<bool>("Deleted")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset?>("DeletedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<DateTimeOffset>("CreatedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Message")
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<bool>("IsRead")
                    .HasColumnType("bit");

                b.Property<string>("Metadata")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("RecipientUserId")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTimeOffset?>("ReadAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("Type")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.HasKey("Id");

                b.ToTable("Notifications");
            });

            modelBuilder.Entity("IMTS.Shared.Models.NotificationPreference", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<bool>("Deleted")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset?>("DeletedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<DateTimeOffset>("CreatedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<bool>("EmailEnabled")
                    .HasColumnType("bit");

                b.Property<string>("NotificationTypes")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<bool>("InAppEnabled")
                    .HasColumnType("bit");

                b.HasKey("Id");

                b.ToTable("NotificationPreferences");
            });

            modelBuilder.Entity("IMTS.Shared.Models.NotificationType", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<bool>("Deleted")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset?>("DeletedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<DateTimeOffset>("CreatedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Icon")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Color")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("NotificationTypes");
            });
        }
    }
}

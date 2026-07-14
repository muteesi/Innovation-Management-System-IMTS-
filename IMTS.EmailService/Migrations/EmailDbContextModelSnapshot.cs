using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using IMTS.Shared.Models;

#nullable disable

namespace IMTS.EmailService.Migrations
{
    [DbContext(typeof(EmailDbContext))]
    partial class EmailDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0");

            modelBuilder.Entity("IMTS.Shared.Models.EmailLog", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("CreatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<bool>("Deleted")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset>("CreatedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ErrorMessage")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("Subject")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("SentAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ToAddress")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("EmailLogs");
            });

            modelBuilder.Entity("IMTS.Shared.Models.EmailQueueItem", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("BodyHtml")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("CreatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<bool>("Deleted")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset>("CreatedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ErrorMessage")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<int>("RetryCount")
                    .HasColumnType("int");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("Subject")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<DateTimeOffset?>("SentAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("TemplateKey")
                    .HasColumnType("nvarchar(100)")
                    .HasMaxLength(100);

                b.Property<string>("ToAddress")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.HasKey("Id");

                b.ToTable("EmailQueueItems");
            });

            modelBuilder.Entity("IMTS.Shared.Models.EmailTemplate", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("BodyHtml")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("CreatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<bool>("Deleted")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset>("CreatedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("Subject")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("TemplateKey")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.ToTable("EmailTemplates");
            });
        }
    }
}

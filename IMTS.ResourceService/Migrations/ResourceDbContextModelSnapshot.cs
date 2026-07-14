using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using IMTS.Shared.Models;

#nullable disable

namespace IMTS.ResourceService.Migrations
{
    [DbContext(typeof(ResourceDbContext))]
    partial class ResourceDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0");

            modelBuilder.Entity("IMTS.Shared.Models.Resource", b =>
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
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<bool>("IsFeatured")
                    .HasColumnType("bit");

                b.Property<string>("FileName")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("FileType")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<long>("FileSizeBytes")
                    .HasColumnType("bigint");

                b.Property<string>("FileUrl")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("PreviewMetadata")
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("DownloadCount")
                    .HasColumnType("int");

                b.Property<string>("Category")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Slug")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("PublishedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("StoragePath")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Version")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.HasKey("Id");

                b.HasIndex("Category");

                b.ToTable("Resources");
            });

            modelBuilder.Entity("IMTS.Shared.Models.ResourceCategory", b =>
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

                b.Property<int>("SortOrder")
                    .HasColumnType("int");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Slug")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("ResourceCategories");
            });

            modelBuilder.Entity("IMTS.Shared.Models.ResourceVersion", b =>
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

                b.Property<long>("FileSizeBytes")
                    .HasColumnType("bigint");

                b.Property<string>("FileName")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("VersionNumber")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("StoragePath")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("ChangeSummary")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("ResourceId")
                    .HasColumnType("int");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("ResourceVersions");
            });

            modelBuilder.Entity("IMTS.Shared.Models.DownloadLog", b =>
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

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("UserAgent")
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("ResourceId")
                    .HasColumnType("int");

                b.Property<string>("IpAddress")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("ModifiedDate")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ModifiedBy")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("DownloadLogs");
            });
        }
    }
}

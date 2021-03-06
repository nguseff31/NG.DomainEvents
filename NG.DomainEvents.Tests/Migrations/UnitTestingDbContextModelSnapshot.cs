// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NG.DomainEvents.Tests.Fixtures;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NG.DomainEvents.Tests.Migrations
{
    [DbContext(typeof(UnitTestingDbContext))]
    partial class UnitTestingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NG.DomainEvents.Data.DomainEventDto", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("EntityId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<string>("EntityTableName")
                        .IsRequired()
                        .HasMaxLength(63)
                        .HasColumnType("character varying(63)");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<int>("Retries")
                        .HasColumnType("integer");

                    b.Property<bool>("ShouldExecute")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("domain_event");
                });

            modelBuilder.Entity("NG.DomainEvents.Data.DomainEventResultDto", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("Completed")
                        .HasColumnType("boolean");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("DateExecuted")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("DomainEventId")
                        .HasColumnType("bigint");

                    b.Property<TimeSpan>("Elapsed")
                        .HasColumnType("interval");

                    b.Property<string>("Handler")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("DomainEventId");

                    b.ToTable("domain_event_result");
                });

            modelBuilder.Entity("NG.DomainEvents.Tests.Model.ArticleDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Published")
                        .HasColumnType("boolean");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("article");
                });

            modelBuilder.Entity("NG.DomainEvents.Data.DomainEventResultDto", b =>
                {
                    b.HasOne("NG.DomainEvents.Data.DomainEventDto", "DomainEvent")
                        .WithMany("Results")
                        .HasForeignKey("DomainEventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DomainEvent");
                });

            modelBuilder.Entity("NG.DomainEvents.Data.DomainEventDto", b =>
                {
                    b.Navigation("Results");
                });
#pragma warning restore 612, 618
        }
    }
}

using System;
using System.Collections.Generic;
using EfExample.Models;
using Microsoft.EntityFrameworkCore;

namespace EfExample.Context;

public partial class ApbdContext : DbContext
{
    public ApbdContext()
    {
    }

    public ApbdContext(DbContextOptions<ApbdContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }
    
    public virtual DbSet<Country> Countries { get; set; }
    
    public virtual DbSet<ClientTrip> ClientTrips { get; set; }

    public virtual DbSet<CountryTrip> CountryTrips { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer(
                "Data Source=apbdus.database.windows.net;Initial Catalog=apbd_us;User Id=boguslaw;Password={secretPass};Encrypt=False")
            .LogTo(Console.WriteLine, LogLevel.Information);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdClient).HasName("Client_pk");

            entity.ToTable("Client", "trip");

            entity.Property(e => e.FirstName).HasMaxLength(120);
            entity.Property(e => e.LastName).HasMaxLength(120);
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.Telephone).HasMaxLength(120);
            entity.Property(e => e.Pesel).HasMaxLength(120);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.IdCountry).HasName("Country_pk");

            entity.ToTable("Country", "trip");

            entity.Property(e => e.Name).HasMaxLength(120);
        });
        
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.IdTrip).HasName("Trip_pk");

            entity.ToTable("Trip", "trip");

            entity.Property(e => e.Name).HasMaxLength(120);
            entity.Property(e => e.Description).HasMaxLength(220);
            entity.Property(e => e.DateFrom).HasColumnType("datetime");
            entity.Property(e => e.DateTo).HasColumnType("datetime");
            entity.Property(e => e.MaxPeople).HasColumnType("int");
        });

        modelBuilder.Entity<CountryTrip>(entity =>
        {
            entity.HasKey(e => new { e.IdCountry, e.IdTrip }).HasName("Country_Trip_pk");

            entity.ToTable("Country_Trip", "trip");

            entity.HasOne(d => d.IdCountryNavigation).WithMany(p => p.CountryTrips)
                .HasForeignKey(d => d.IdCountry)
                .HasConstraintName("Country_Trip_Country");

            entity.HasOne(d => d.IdTripNavigation).WithMany(p => p.CountryTrips)
                .HasForeignKey(d => d.IdTrip)
                .HasConstraintName("Country_Trip_Trip");
        });
        
        modelBuilder.Entity<ClientTrip>(entity =>
        {
            entity.HasKey(e => new { e.IdClient, e.IdTrip }).HasName("Client_Trip_pk");
            
            entity.ToTable("Client_Trip", "trip");

            entity.Property(e => e.RegisteredAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.ClientTrips)
                .HasForeignKey(d => d.IdClient)
                .HasConstraintName("Table_5_Client");

            entity.HasOne(d => d.IdTripNavigation).WithMany(p => p.ClientTrips)
                .HasForeignKey(d => d.IdTrip)
                .HasConstraintName("Table_5_Trip");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

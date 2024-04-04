using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Domain.Entities;


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace CielaDocs.Data.Contexts
{
    public class ApplicationDbContext :IdentityDbContext, IApplicationDbContext
    {
        private readonly IDateTime _dateTime;
        private IDbContextTransaction _currentTransaction;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
       
     
      
        public virtual DbSet<Country> Countries { get; set; }
     
        public virtual DbSet<GroupTable> GroupTables { get; set; }
     
        public virtual DbSet<LocalArea> LocalAreas { get; set; }
        public virtual DbSet<Municipality> Municipalities { get; set; }
       
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Reports> Reports { get; set; }
       
        public virtual DbSet<Town> Towns { get; set; }
       
        public virtual DbSet<Feedback> Feedback { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
        public virtual DbSet<InstitutionType> InstitutionTypes { get; set; }
        public virtual DbSet<CourtType> CourtTypes { get; set; }
        public virtual  DbSet<Court> Courts { get; set; }
        public virtual DbSet<Section> Section { get; set; }
        public virtual DbSet<EbkPar> EbkPar { get; set; }
        public virtual DbSet<EbkSubPar> EbkSubPar { get; set; }
        public virtual DbSet<FunctionalSubArea> FunctionalSubArea { get; set; }
        public virtual DbSet<FunctionalArea> FunctionalArea { get; set; }
        public virtual DbSet<Measure> Measure { get; set; }
        public virtual DbSet<MainIndicators> MainIndicators { get; set; }
        public virtual DbSet<FinYear> FinYear { get; set; }
        public virtual DbSet<FunctionalAction> FunctionalAction { get; set; }
        public virtual DbSet<Test> Test { get; set; }
        public virtual DbSet<Test2> Test2{ get; set; }
        public virtual DbSet<Metrics> Metrics { get; set; }
        public virtual DbSet<MetricsField> MetricsField { get; set; }
        public virtual DbSet<MainDataItem> MainDataItems { get; set; } = null!;
        public virtual DbSet<MainData> MainData { get; set; } = null!;
        public virtual DbSet<MainPeriodItem> MainPeriodItem { get; set; }
        public virtual DbSet<MainPeriod> MainPeriod { get; set; }
        public virtual DbSet<ProgramDef> ProgramDef { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTime dateTime) : base(options)
        {
            _dateTime = dateTime;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await base.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync().ConfigureAwait(false);

                _currentTransaction?.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
           
         
            modelBuilder.Entity<Feedback>().ToTable("Feedback");
            modelBuilder.Entity<Country>().ToTable("Country");
            modelBuilder.Entity<GroupTable>().ToTable("GroupTable");
            modelBuilder.Entity<LocalArea>().ToTable("LocalArea");
            modelBuilder.Entity<Municipality>().ToTable("Municipality");
            modelBuilder.Entity<Region>().ToTable("Region");
            modelBuilder.Entity<Reports>().ToTable("Reports");
            modelBuilder.Entity<Town>().ToTable("Town");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserType>().ToTable("UserType");
            modelBuilder.Entity<InstitutionType>().ToTable("InstitutionType");
            modelBuilder.Entity<CourtType>().ToTable("CourtType");
            modelBuilder.Entity<Court>().ToTable("Court");
            modelBuilder.Entity<Section>().ToTable("Section");
            modelBuilder.Entity<EbkPar>().ToTable("EbkPar");
            modelBuilder.Entity<EbkSubPar>().ToTable("EbkSubPar");
            modelBuilder.Entity<FunctionalArea>().ToTable("FunctionalArea");
            modelBuilder.Entity<FunctionalSubArea>().ToTable("FunctionalSubArea");
            modelBuilder.Entity<Measure>().ToTable("Measure");
            modelBuilder.Entity<MainIndicators>().ToTable("MainIndicators");
            modelBuilder.Entity<FinYear>().ToTable("FinYear");
            modelBuilder.Entity<FunctionalAction>().ToTable("FunctionalAction");
            modelBuilder.Entity<Metrics>().ToTable("Metrics");
            modelBuilder.Entity<MetricsField>().ToTable("MetricsField");
            modelBuilder.Entity<MainData>().ToTable("MainData");
            modelBuilder.Entity<MainDataItem>().ToTable("MainDataItem");
            modelBuilder.Entity<MainPeriod>().ToTable("MainPeriod");
            modelBuilder.Entity<MainPeriodItem>().ToTable("MainPeriodItem");
            modelBuilder.Entity<ProgramDef>().ToTable("ProgramDef");
            modelBuilder.Entity<Test>().ToTable("Test");
            modelBuilder.Entity<Test2>().ToTable("Test2");
            base.OnModelCreating(modelBuilder);
        }

       
    }
}

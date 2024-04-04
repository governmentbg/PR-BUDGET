using CielaDocs.Domain.Entities;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DatabaseFacade Database { get; }
     
       
      
         DbSet<Country> Countries { get; set; }
         DbSet<User> Users { get; set; }
         DbSet<UserType> UserTypes { get; set; }
         DbSet<GroupTable> GroupTables { get; set; }
         DbSet<LocalArea> LocalAreas { get; set; }
         DbSet<Municipality> Municipalities { get; set; }
         DbSet<Region> Regions { get; set; }
         DbSet<Reports> Reports { get; set; }
        DbSet<Town> Towns { get; set; }
        DbSet<Feedback> Feedback { get; set; }
        DbSet<InstitutionType> InstitutionTypes { get; set; }
        DbSet<CourtType> CourtTypes { get; set; }
        DbSet<Court> Courts { get; set; }
        DbSet<Section> Section { get; set; }
        DbSet<EbkPar> EbkPar { get; set; }
        DbSet<EbkSubPar> EbkSubPar { get; set; }
        DbSet<FunctionalSubArea> FunctionalSubArea { get; set; }
        DbSet<FunctionalArea> FunctionalArea { get; set; }
        DbSet<Measure> Measure { get; set; }
        DbSet<MainIndicators> MainIndicators { get; set; }
        DbSet<FinYear> FinYear { get; set; }
        DbSet<FunctionalAction> FunctionalAction { get; set; }
        DbSet<Metrics> Metrics { get; set; }
        DbSet<MetricsField> MetricsField { get; set; }
        DbSet<MainDataItem> MainDataItems { get; set; } 
        DbSet<MainData> MainData { get; set; }
        DbSet<MainPeriodItem> MainPeriodItem { get; set; }
        DbSet<MainPeriod> MainPeriod { get; set; }
        DbSet<ProgramDef> ProgramDef { get; set; }
        DbSet<Test> Test { get; set; }
        DbSet<Test2> Test2 { get; set; }
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        void RollbackTransaction();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
      
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CielaDocs.Data.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    EmplId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllDay = table.Column<bool>(type: "bit", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    DocNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SigId = table.Column<int>(type: "int", nullable: false),
                    DocKindOfId = table.Column<int>(type: "int", nullable: false),
                    DocTypeId = table.Column<int>(type: "int", nullable: false),
                    ReceivedTypeId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: true),
                    SenderId = table.Column<int>(type: "int", nullable: true),
                    CorrId = table.Column<int>(type: "int", nullable: true),
                    CorrName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrTownId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrDocNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardStatusId = table.Column<int>(type: "int", nullable: true),
                    IsResponseNeeded = table.Column<bool>(type: "bit", nullable: true),
                    DaysToResponse = table.Column<int>(type: "int", nullable: true),
                    IsSecured = table.Column<bool>(type: "bit", nullable: true),
                    CardMasterId = table.Column<long>(type: "bigint", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardOutId = table.Column<long>(type: "bigint", nullable: true),
                    AssignedToId = table.Column<long>(type: "bigint", nullable: true),
                    CardRouteId = table.Column<int>(type: "int", nullable: true),
                    IsRealCard = table.Column<bool>(type: "bit", nullable: true),
                    CardGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TmpDocNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TmpDocDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true),
                    TypesOfDocumentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendingAndReceivingTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrTownName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardStatusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SigName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentDocNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentDocDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardDocument",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    CardGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TmpGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsInitialDoc = table.Column<bool>(type: "bit", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: true),
                    LockedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActionLocked = table.Column<bool>(type: "bit", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StartSchoolYear = table.Column<int>(type: "int", nullable: false),
                    EndSchoolYear = table.Column<int>(type: "int", nullable: false),
                    NewSchoolYearDateAuto = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cfg", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Depart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    InactiveFromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DirectType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    SendEmailToEmpl = table.Column<bool>(type: "bit", nullable: true),
                    IncludeInCoordinationList = table.Column<bool>(type: "bit", nullable: true),
                    AutoLockDocs = table.Column<bool>(type: "bit", nullable: true),
                    InternalUsage = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmplDirectDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUserId = table.Column<int>(type: "int", nullable: true),
                    TmpGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanDeleteDoc = table.Column<bool>(type: "bit", nullable: true),
                    EmplDirectGuid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmplDirectDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmplMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    TmpGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionLevel = table.Column<int>(type: "int", nullable: true),
                    CompletionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerId = table.Column<long>(type: "bigint", nullable: true),
                    ScheduledStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmplMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalArea",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalArea", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipality",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ekatte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipality", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nkpdposition",
                columns: table => new
                {
                    NkpdpositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NkpdsubGroupId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffTypeId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nkpdposition", x => x.NkpdpositionId);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegCommon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    StorePeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegCommon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ekatte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegPriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegPriority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportSubTitle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "School",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstitutionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Abbreviation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    OblId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObstId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NasmeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebSite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EdeliveryEnabled = table.Column<bool>(type: "bit", nullable: true),
                    EdeliveryEnabledOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EdeliveryCert = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EdeliveryCertExpiredOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EdeliveryThumbprint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EdeliveryCertPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EdeliverySigId = table.Column<int>(type: "int", nullable: true),
                    CanClerkResponseOnBehalfOnEmpl = table.Column<bool>(type: "bit", nullable: true),
                    EdeliveryAutoRegister = table.Column<bool>(type: "bit", nullable: true),
                    EmailNotificationEnabled = table.Column<bool>(type: "bit", nullable: true),
                    SchoolYearsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_School", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SendingAndReceivingType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingAndReceivingType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatCardData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Num = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatCardData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    CardId = table.Column<long>(type: "bigint", nullable: true),
                    SenderEmplId = table.Column<int>(type: "int", nullable: true),
                    RecipientEmplId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TmpGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmplMessageId = table.Column<long>(type: "bigint", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionLevel = table.Column<int>(type: "int", nullable: true),
                    CompletionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskStatusName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Town",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Oblast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Obstina = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kmetstvo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Town", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypesOfDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesOfDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    DepartId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    Passw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoginName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoginEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsManager = table.Column<bool>(type: "bit", nullable: false),
                    IsEmploye = table.Column<bool>(type: "bit", nullable: false),
                    IsClerk = table.Column<bool>(type: "bit", nullable: false),
                    IsRepresentDepart = table.Column<bool>(type: "bit", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanAssignmentTask = table.Column<bool>(type: "bit", nullable: false),
                    CanTerminateDeloProcedure = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true),
                    InactiveFromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicEduNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SysUserId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstitutionId = table.Column<int>(type: "int", nullable: true),
                    StaffPositionId = table.Column<int>(type: "int", nullable: true),
                    StaffTypeId = table.Column<int>(type: "int", nullable: true),
                    NkpdpositionId = table.Column<int>(type: "int", nullable: true),
                    PositionKindId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empl_Depart_DepartId",
                        column: x => x.DepartId,
                        principalTable: "Depart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Empl_Nkpdposition_NkpdpositionId",
                        column: x => x.NkpdpositionId,
                        principalTable: "Nkpdposition",
                        principalColumn: "NkpdpositionId");
                    table.ForeignKey(
                        name: "FK_Empl_Position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Position",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Empl_School_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    RegCommonId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegNum = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    IsLeadingZero = table.Column<bool>(type: "bit", nullable: true),
                    ExpandedLength = table.Column<int>(type: "int", nullable: true),
                    RegGroupId = table.Column<int>(type: "int", nullable: true),
                    RegPriorityID = table.Column<int>(type: "int", nullable: true),
                    TmpNum = table.Column<int>(type: "int", nullable: true),
                    CanIssueTmpNum = table.Column<bool>(type: "bit", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SchoolYearsId = table.Column<int>(type: "int", nullable: true),
                    StorePeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaveNumForNewSchoolYear = table.Column<bool>(type: "bit", nullable: true),
                    ResetDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reg_RegCommon_RegCommonId",
                        column: x => x.RegCommonId,
                        principalTable: "RegCommon",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reg_RegGroup_RegGroupId",
                        column: x => x.RegGroupId,
                        principalTable: "RegGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reg_RegPriority_RegPriorityID",
                        column: x => x.RegPriorityID,
                        principalTable: "RegPriority",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reg_School_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "School",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SchoolYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    StartYear = table.Column<int>(type: "int", nullable: true),
                    EndYear = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolYears_School_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Correspondent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    Identifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    TownId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correspondent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Correspondent_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Correspondent_School_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "School",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Correspondent_Town_TownId",
                        column: x => x.TownId,
                        principalTable: "Town",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmplDirect",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    DirectTypeId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    SenderDepartId = table.Column<int>(type: "int", nullable: true),
                    RecipientDepartId = table.Column<int>(type: "int", nullable: true),
                    SenderEmplId = table.Column<int>(type: "int", nullable: true),
                    RecipientEmplId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    IsReaded = table.Column<bool>(type: "bit", nullable: true),
                    IsReply = table.Column<bool>(type: "bit", nullable: true),
                    TmpGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmplMessageId = table.Column<long>(type: "bigint", nullable: true),
                    AnswerId = table.Column<long>(type: "bigint", nullable: true),
                    EmplProcedureId = table.Column<int>(type: "int", nullable: true),
                    AnswerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmplRouteTemplateId = table.Column<int>(type: "int", nullable: true),
                    PerformedAction = table.Column<bool>(type: "bit", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmplDirect", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmplDirect_DirectType_DirectTypeId",
                        column: x => x.DirectTypeId,
                        principalTable: "DirectType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmplDirect_EmplMessage_EmplMessageId",
                        column: x => x.EmplMessageId,
                        principalTable: "EmplMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmplDirect_Empl_RecipientEmplId",
                        column: x => x.RecipientEmplId,
                        principalTable: "Empl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmplDirect_Empl_SenderEmplId",
                        column: x => x.SenderEmplId,
                        principalTable: "Empl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmplDirect_School_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LetterOfAttorney",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Onr = table.Column<int>(type: "int", nullable: true),
                    EmplId = table.Column<int>(type: "int", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmplSubstituteId = table.Column<int>(type: "int", nullable: true),
                    TmpGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CanRead = table.Column<bool>(type: "bit", nullable: true),
                    CanEdit = table.Column<bool>(type: "bit", nullable: true),
                    CanDelete = table.Column<bool>(type: "bit", nullable: true),
                    CanAuthor = table.Column<bool>(type: "bit", nullable: true),
                    CanResolution = table.Column<bool>(type: "bit", nullable: true),
                    CanSign = table.Column<bool>(type: "bit", nullable: true),
                    CanAgree = table.Column<bool>(type: "bit", nullable: true),
                    CanTask = table.Column<bool>(type: "bit", nullable: true),
                    OrderNum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LetterOfAttorney", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LetterOfAttorney_Empl_EmplSubstituteId",
                        column: x => x.EmplSubstituteId,
                        principalTable: "Empl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    DocNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SigId = table.Column<int>(type: "int", nullable: false),
                    DocKindOfId = table.Column<int>(type: "int", nullable: false),
                    DocTypeId = table.Column<int>(type: "int", nullable: false),
                    ReceivedTypeId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: true),
                    SenderId = table.Column<int>(type: "int", nullable: true),
                    CorrId = table.Column<int>(type: "int", nullable: true),
                    CorrName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrTownId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrDocNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardStatusId = table.Column<int>(type: "int", nullable: true),
                    IsResponseNeeded = table.Column<bool>(type: "bit", nullable: true),
                    DaysToResponse = table.Column<int>(type: "int", nullable: true),
                    IsSecured = table.Column<bool>(type: "bit", nullable: true),
                    CardMasterId = table.Column<long>(type: "bigint", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardOutId = table.Column<long>(type: "bigint", nullable: true),
                    AssignedToId = table.Column<long>(type: "bigint", nullable: true),
                    CardRouteId = table.Column<int>(type: "int", nullable: true),
                    IsRealCard = table.Column<bool>(type: "bit", nullable: true),
                    CardGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TmpDocNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TmpDocDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true),
                    EdeliveryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Card_CardStatus_CardStatusId",
                        column: x => x.CardStatusId,
                        principalTable: "CardStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Card_Correspondent_CorrId",
                        column: x => x.CorrId,
                        principalTable: "Correspondent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Card_DocType_DocTypeId",
                        column: x => x.DocTypeId,
                        principalTable: "DocType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_Reg_SigId",
                        column: x => x.SigId,
                        principalTable: "Reg",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_School_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_SendingAndReceivingType_ReceivedTypeId",
                        column: x => x.ReceivedTypeId,
                        principalTable: "SendingAndReceivingType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_TypesOfDocument_DocKindOfId",
                        column: x => x.DocKindOfId,
                        principalTable: "TypesOfDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Card_CardStatusId",
                table: "Card",
                column: "CardStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_CorrId",
                table: "Card",
                column: "CorrId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_DocKindOfId",
                table: "Card",
                column: "DocKindOfId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_DocTypeId",
                table: "Card",
                column: "DocTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_ReceivedTypeId",
                table: "Card",
                column: "ReceivedTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_SchoolId",
                table: "Card",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_SigId",
                table: "Card",
                column: "SigId");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondent_CountryId",
                table: "Correspondent",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondent_SchoolId",
                table: "Correspondent",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondent_TownId",
                table: "Correspondent",
                column: "TownId");

            migrationBuilder.CreateIndex(
                name: "IX_Empl_DepartId",
                table: "Empl",
                column: "DepartId");

            migrationBuilder.CreateIndex(
                name: "IX_Empl_NkpdpositionId",
                table: "Empl",
                column: "NkpdpositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Empl_PositionId",
                table: "Empl",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Empl_SchoolId",
                table: "Empl",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_EmplDirect_DirectTypeId",
                table: "EmplDirect",
                column: "DirectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmplDirect_EmplMessageId",
                table: "EmplDirect",
                column: "EmplMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmplDirect_RecipientEmplId",
                table: "EmplDirect",
                column: "RecipientEmplId");

            migrationBuilder.CreateIndex(
                name: "IX_EmplDirect_SchoolId",
                table: "EmplDirect",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_EmplDirect_SenderEmplId",
                table: "EmplDirect",
                column: "SenderEmplId");

            migrationBuilder.CreateIndex(
                name: "IX_LetterOfAttorney_EmplSubstituteId",
                table: "LetterOfAttorney",
                column: "EmplSubstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reg_RegCommonId",
                table: "Reg",
                column: "RegCommonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reg_RegGroupId",
                table: "Reg",
                column: "RegGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Reg_RegPriorityID",
                table: "Reg",
                column: "RegPriorityID");

            migrationBuilder.CreateIndex(
                name: "IX_Reg_SchoolId",
                table: "Reg",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolYears_SchoolId",
                table: "SchoolYears",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropTable(
                name: "CardDetails");

            migrationBuilder.DropTable(
                name: "CardDocument");

            migrationBuilder.DropTable(
                name: "Cfg");

            migrationBuilder.DropTable(
                name: "EmplDirect");

            migrationBuilder.DropTable(
                name: "EmplDirectDocument");

            migrationBuilder.DropTable(
                name: "LetterOfAttorney");

            migrationBuilder.DropTable(
                name: "LocalArea");

            migrationBuilder.DropTable(
                name: "Municipality");

            migrationBuilder.DropTable(
                name: "Region");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "SchoolYears");

            migrationBuilder.DropTable(
                name: "StatCardData");

            migrationBuilder.DropTable(
                name: "TaskData");

            migrationBuilder.DropTable(
                name: "TaskStatus");

            migrationBuilder.DropTable(
                name: "CardStatus");

            migrationBuilder.DropTable(
                name: "Correspondent");

            migrationBuilder.DropTable(
                name: "DocType");

            migrationBuilder.DropTable(
                name: "Reg");

            migrationBuilder.DropTable(
                name: "SendingAndReceivingType");

            migrationBuilder.DropTable(
                name: "TypesOfDocument");

            migrationBuilder.DropTable(
                name: "DirectType");

            migrationBuilder.DropTable(
                name: "EmplMessage");

            migrationBuilder.DropTable(
                name: "Empl");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "Town");

            migrationBuilder.DropTable(
                name: "RegCommon");

            migrationBuilder.DropTable(
                name: "RegGroup");

            migrationBuilder.DropTable(
                name: "RegPriority");

            migrationBuilder.DropTable(
                name: "Depart");

            migrationBuilder.DropTable(
                name: "Nkpdposition");

            migrationBuilder.DropTable(
                name: "Position");

            migrationBuilder.DropTable(
                name: "School");
        }
    }
}

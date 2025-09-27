using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinking.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSinkingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    OldValues = table.Column<string>(type: "TEXT", nullable: true),
                    NewValues = table.Column<string>(type: "TEXT", nullable: true),
                    IPAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalAccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SourceSystem = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EncryptedToken = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastTestResult = table.Column<bool>(type: "INTEGER", nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastTestMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalAccessTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SourceSystem = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetSystem = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceProject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TargetProject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CronExpression = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPaused = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncJobs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SyncJobs_PersonalAccessTokens_SourceTokenId",
                        column: x => x.SourceTokenId,
                        principalTable: "PersonalAccessTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncJobs_PersonalAccessTokens_TargetTokenId",
                        column: x => x.TargetTokenId,
                        principalTable: "PersonalAccessTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FieldMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceField = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TargetField = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ValueMapping = table.Column<string>(type: "TEXT", nullable: true),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldMappings_SyncJobs_SyncJobId",
                        column: x => x.SyncJobId,
                        principalTable: "SyncJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncJobExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalItems = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessedItems = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedItems = table.Column<int>(type: "INTEGER", nullable: false),
                    ExecutionLog = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncJobExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncJobExecutions_SyncJobs_SyncJobId",
                        column: x => x.SyncJobId,
                        principalTable: "SyncJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncJobFailures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExecutionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceItemId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SourceItemTitle = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SourceItemUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TargetItemId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FailureReason = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorDetails = table.Column<string>(type: "TEXT", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncJobFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncJobFailures_SyncJobExecutions_ExecutionId",
                        column: x => x.ExecutionId,
                        principalTable: "SyncJobExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_ChangedAt",
                table: "ChangeLogs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_UserId",
                table: "ChangeLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldMappings_SyncJobId",
                table: "FieldMappings",
                column: "SyncJobId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccessTokens_UserId",
                table: "PersonalAccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobExecutions_Status",
                table: "SyncJobExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobExecutions_SyncJobId",
                table: "SyncJobExecutions",
                column: "SyncJobId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobFailures_ExecutionId",
                table: "SyncJobFailures",
                column: "ExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_IsActive",
                table: "SyncJobs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_SourceTokenId",
                table: "SyncJobs",
                column: "SourceTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_TargetTokenId",
                table: "SyncJobs",
                column: "TargetTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_UserId",
                table: "SyncJobs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeLogs");

            migrationBuilder.DropTable(
                name: "FieldMappings");

            migrationBuilder.DropTable(
                name: "SyncJobFailures");

            migrationBuilder.DropTable(
                name: "SyncJobExecutions");

            migrationBuilder.DropTable(
                name: "SyncJobs");

            migrationBuilder.DropTable(
                name: "PersonalAccessTokens");
        }
    }
}

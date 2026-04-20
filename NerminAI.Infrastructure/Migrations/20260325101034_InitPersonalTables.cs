using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NerminAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPersonalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyRoutines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeOfDay = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Activity = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ShadowDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyRoutines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RelatedMemoryTitles = table.Column<string[]>(type: "text[]", nullable: false),
                    ShadowDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LifeEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EmotionalSignificance = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    ShadowDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Memories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MemoryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmotionalTone = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ShadowDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ToneStyle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SpeakingStyle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PersonalityTraits = table.Column<string[]>(type: "text[]", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ShadowDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Item = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Strength = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShadowDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClosenessLevel = table.Column<int>(type: "integer", nullable: false),
                    ShadowDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chunks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Embeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Vector = table.Column<float[]>(type: "real[]", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Embeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Embeddings_Chunks_ChunkId",
                        column: x => x.ChunkId,
                        principalTable: "Chunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_DocumentId",
                table: "Chunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_DocumentId_ChunkIndex",
                table: "Chunks",
                columns: new[] { "DocumentId", "ChunkIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyRoutines_TimeOfDay",
                table: "DailyRoutines",
                column: "TimeOfDay");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatedAt",
                table: "Documents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Type",
                table: "Documents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Embeddings_ChunkId",
                table: "Embeddings",
                column: "ChunkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interests_Level",
                table: "Interests",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_LifeEvents_Date",
                table: "LifeEvents",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_LifeEvents_EmotionalSignificance",
                table: "LifeEvents",
                column: "EmotionalSignificance");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_Category",
                table: "Memories",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_EmotionalTone",
                table: "Memories",
                column: "EmotionalTone");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_MemoryDate",
                table: "Memories",
                column: "MemoryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfiles_IsActive",
                table: "PersonProfiles",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_Category",
                table: "Preferences",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_Strength",
                table: "Preferences",
                column: "Strength");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ClosenessLevel",
                table: "Relationships",
                column: "ClosenessLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_Type",
                table: "Relationships",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyRoutines");

            migrationBuilder.DropTable(
                name: "Embeddings");

            migrationBuilder.DropTable(
                name: "Interests");

            migrationBuilder.DropTable(
                name: "LifeEvents");

            migrationBuilder.DropTable(
                name: "Memories");

            migrationBuilder.DropTable(
                name: "PersonProfiles");

            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropTable(
                name: "Chunks");

            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}

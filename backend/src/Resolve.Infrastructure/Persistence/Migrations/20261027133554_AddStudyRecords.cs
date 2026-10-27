using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolve.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "study_records",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    learning_item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    result = table.Column<string>(type: "TEXT", nullable: false),
                    studied_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    duration_seconds = table.Column<int>(type: "INTEGER", nullable: true),
                    memo = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_records_learning_items_learning_item_id",
                        column: x => x.learning_item_id,
                        principalTable: "learning_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_study_records_learning_item_id_studied_at",
                table: "study_records",
                columns: new[] { "learning_item_id", "studied_at" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "study_records");
        }
    }
}

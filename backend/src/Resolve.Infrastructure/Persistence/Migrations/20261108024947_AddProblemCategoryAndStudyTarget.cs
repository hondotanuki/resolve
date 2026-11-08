using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolve.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemCategoryAndStudyTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing records have no saved target; resetting the development database is required.
            migrationBuilder.Sql(
                "CREATE TEMP TABLE require_empty_study_records (count INTEGER NOT NULL CHECK (count = 0));"
            );
            migrationBuilder.Sql(
                "INSERT INTO require_empty_study_records SELECT (SELECT COUNT(*) FROM study_records) + (SELECT COUNT(*) FROM learning_items);"
            );
            migrationBuilder.Sql("DROP TABLE require_empty_study_records;");
            migrationBuilder.AlterColumn<int>(
                name: "duration_seconds",
                table: "study_records",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "target_seconds",
                table: "study_records",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "learning_items",
                type: "TEXT",
                nullable: false
            );

            migrationBuilder.AddColumn<string>(
                name: "problem_index",
                table: "learning_items",
                type: "TEXT",
                nullable: false
            );

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "learning_items",
                type: "TEXT",
                nullable: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "target_seconds", table: "study_records");

            migrationBuilder.DropColumn(name: "category", table: "learning_items");

            migrationBuilder.DropColumn(name: "problem_index", table: "learning_items");

            migrationBuilder.DropColumn(name: "source", table: "learning_items");

            migrationBuilder.AlterColumn<int>(
                name: "duration_seconds",
                table: "study_records",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );
        }
    }
}

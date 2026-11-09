using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolve.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RequireProblemMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing incomplete items must be fixed or the development database recreated.
            migrationBuilder.Sql(
                "CREATE TEMP TABLE require_problem_metadata (count INTEGER NOT NULL CHECK (count = 0));"
            );
            migrationBuilder.Sql(
                "INSERT INTO require_problem_metadata SELECT COUNT(*) FROM learning_items WHERE problem_url IS NULL OR trim(problem_url) = '' OR contest_id IS NULL OR trim(contest_id) = '' OR contest_name IS NULL OR trim(contest_name) = '' OR contest_start_date IS NULL OR contest_start_date = '0001-01-01';"
            );
            migrationBuilder.Sql("DROP TABLE require_problem_metadata;");
            migrationBuilder.AlterColumn<string>(
                name: "problem_url",
                table: "learning_items",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateOnly>(
                name: "contest_start_date",
                table: "learning_items",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "contest_name",
                table: "learning_items",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "contest_id",
                table: "learning_items",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "problem_url",
                table: "learning_items",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<DateOnly>(
                name: "contest_start_date",
                table: "learning_items",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "contest_name",
                table: "learning_items",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "contest_id",
                table: "learning_items",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );
        }
    }
}

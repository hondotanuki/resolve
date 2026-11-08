using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolve.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningItemContestInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "contest_id",
                table: "learning_items",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "contest_name",
                table: "learning_items",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<DateOnly>(
                name: "contest_start_date",
                table: "learning_items",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "problem_url",
                table: "learning_items",
                type: "TEXT",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "contest_id", table: "learning_items");

            migrationBuilder.DropColumn(name: "contest_name", table: "learning_items");

            migrationBuilder.DropColumn(name: "contest_start_date", table: "learning_items");

            migrationBuilder.DropColumn(name: "problem_url", table: "learning_items");
        }
    }
}

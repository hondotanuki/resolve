using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolve.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueProblemUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_learning_items_problem_url",
                table: "learning_items",
                column: "problem_url",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_learning_items_problem_url",
                table: "learning_items"
            );
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTrackerBC.Migrations
{
    /// <inheritdoc />
    public partial class migration002addedprioritypropertytoproject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Projects");
        }
    }
}

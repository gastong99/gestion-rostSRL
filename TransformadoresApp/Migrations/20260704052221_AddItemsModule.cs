using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransformadoresApp.Migrations
{
    /// <inheritdoc />
    public partial class AddItemsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemType",
                table: "Categories",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "Categories");
        }
    }
}

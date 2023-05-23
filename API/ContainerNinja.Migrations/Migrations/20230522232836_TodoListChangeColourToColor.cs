using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class TodoListChangeColourToColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colour",
                table: "TodoLists");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TodoLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "TodoLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "TodoLists");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TodoLists",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "TodoLists",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

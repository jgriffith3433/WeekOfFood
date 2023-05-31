using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCookedRecipesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalledIngredient_Recipes_RecipeId",
                table: "CalledIngredient");

            migrationBuilder.AlterColumn<int>(
                name: "RecipeId",
                table: "CalledIngredient",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CookedRecipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CookedRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CookedRecipes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CookedRecipeCalledIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CookedRecipeId = table.Column<int>(type: "int", nullable: false),
                    CalledIngredientId = table.Column<int>(type: "int", nullable: true),
                    ProductStockId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitType = table.Column<int>(type: "int", nullable: false),
                    Units = table.Column<float>(type: "real", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CookedRecipeCalledIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CookedRecipeCalledIngredients_CalledIngredient_CalledIngredientId",
                        column: x => x.CalledIngredientId,
                        principalTable: "CalledIngredient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CookedRecipeCalledIngredients_CookedRecipes_CookedRecipeId",
                        column: x => x.CookedRecipeId,
                        principalTable: "CookedRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CookedRecipeCalledIngredients_ProductStocks_ProductStockId",
                        column: x => x.ProductStockId,
                        principalTable: "ProductStocks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CookedRecipeCalledIngredients_CalledIngredientId",
                table: "CookedRecipeCalledIngredients",
                column: "CalledIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_CookedRecipeCalledIngredients_CookedRecipeId",
                table: "CookedRecipeCalledIngredients",
                column: "CookedRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_CookedRecipeCalledIngredients_ProductStockId",
                table: "CookedRecipeCalledIngredients",
                column: "ProductStockId");

            migrationBuilder.CreateIndex(
                name: "IX_CookedRecipes_RecipeId",
                table: "CookedRecipes",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalledIngredient_Recipes_RecipeId",
                table: "CalledIngredient",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalledIngredient_Recipes_RecipeId",
                table: "CalledIngredient");

            migrationBuilder.DropTable(
                name: "CookedRecipeCalledIngredients");

            migrationBuilder.DropTable(
                name: "CookedRecipes");

            migrationBuilder.AlterColumn<int>(
                name: "RecipeId",
                table: "CalledIngredient",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CalledIngredient_Recipes_RecipeId",
                table: "CalledIngredient",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }
    }
}

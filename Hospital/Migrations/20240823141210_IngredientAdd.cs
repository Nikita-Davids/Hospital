using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class IngredientAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
          name: "ActiveIngredient",
          columns: table => new
          {
              IngredientId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
              IngredientName = table.Column<string>(type: "varchar(max)", nullable: false),
              Strength = table.Column<string>(type: "varchar(max)", nullable: false)
          },
          constraints: table =>
          {
              table.PrimaryKey("PK_ActiveIngredient", x => x.IngredientId);
          });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
          name: "ActiveIngredient");
        }
    }
    }


using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautySalonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStylistToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StylistId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StylistId",
                table: "Orders",
                column: "StylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_StylistId",
                table: "Orders",
                column: "StylistId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_StylistId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StylistId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StylistId",
                table: "Orders");
        }
    }
}

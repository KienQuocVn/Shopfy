using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shofy.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Orders_OrderID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderID",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderID_ReceiverID",
                table: "Messages",
                columns: new[] { "SenderID", "ReceiverID" });

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Orders_OrderID",
                table: "Messages",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Orders_OrderID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderID_ReceiverID",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderID",
                table: "Messages",
                column: "SenderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Orders_OrderID",
                table: "Messages",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductMultipleImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the new images table first, so existing data can be moved into it.
            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ImageContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "SortOrder" });

            // 2. Move each product's existing single image into ProductImages as its primary (SortOrder 0).
            migrationBuilder.Sql(@"
                INSERT INTO ProductImages (Id, ProductId, ImageData, ImageContentType, SortOrder)
                SELECT NEWID(), Id, ImageData, ImageContentType, 0
                FROM Products
                WHERE ImageData IS NOT NULL AND ImageContentType IS NOT NULL;");

            // 3. Drop the now-migrated single-image columns.
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Restore the single-image columns.
            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Products",
                type: "varbinary(max)",
                nullable: true);

            // 2. Copy the primary image (SortOrder 0) back onto each product before the table is dropped.
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.ImageData = pi.ImageData,
                    p.ImageContentType = pi.ImageContentType
                FROM Products p
                INNER JOIN ProductImages pi ON pi.ProductId = p.Id AND pi.SortOrder = 0;");

            // 3. Drop the images table (loses any secondary images — expected on a downgrade).
            migrationBuilder.DropTable(
                name: "ProductImages");
        }
    }
}

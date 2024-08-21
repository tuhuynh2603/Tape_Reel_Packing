using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace TapeReelPacking.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rectanglesModel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    left = table.Column<double>(nullable: false),
                    top = table.Column<double>(nullable: false),
                    Width = table.Column<double>(nullable: false),
                    Height = table.Column<double>(nullable: false),
                    Angle = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rectanglesModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "userlogins",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    L_DeviceLocationRoi = table.Column<string>(type: "varchar(max)", nullable: true),
                    L_LocationEnable = table.Column<bool>(nullable: false),
                    L_ThresholdType = table.Column<string>(nullable: false),
                    L_ObjectColor = table.Column<string>(nullable: false),
                    L_lowerThreshold = table.Column<int>(nullable: false),
                    L_upperThreshold = table.Column<int>(nullable: false),
                    L_lowerThresholdInnerChip = table.Column<int>(nullable: false),
                    L_upperThresholdInnerChip = table.Column<int>(nullable: false),
                    L_OpeningMask = table.Column<int>(nullable: false),
                    L_DilationMask = table.Column<int>(nullable: false),
                    L_MinWidthDevice = table.Column<int>(nullable: false),
                    L_MinHeightDevice = table.Column<int>(nullable: false),
                    L_TemplateRoiId = table.Column<int>(nullable: true),
                    L_NumberSide = table.Column<int>(nullable: false),
                    L_ScaleImageRatio = table.Column<double>(nullable: false),
                    L_MinScore = table.Column<double>(nullable: false),
                    L_CornerIndex = table.Column<int>(nullable: false),
                    DR_NumberROILocation = table.Column<int>(nullable: false),
                    DR_DefectROIIndex = table.Column<string>(nullable: false),
                    OC_lowerThreshold = table.Column<int>(nullable: false),
                    OC_OpeningMask = table.Column<int>(nullable: false),
                    OC_DilationMask = table.Column<int>(nullable: false),
                    OC_MinWidthDevice = table.Column<int>(nullable: false),
                    OC_MinHeightDevice = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userlogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userlogins_rectanglesModel_L_TemplateRoiId",
                        column: x => x.L_TemplateRoiId,
                        principalTable: "rectanglesModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_userlogins_L_TemplateRoiId",
                table: "userlogins",
                column: "L_TemplateRoiId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userlogins");

            migrationBuilder.DropTable(
                name: "rectanglesModel");
        }
    }
}

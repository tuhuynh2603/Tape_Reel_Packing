using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace TapeReelPacking.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userlogins");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "userlogins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    DR_DefectROIIndex = table.Column<string>(type: "text", nullable: false),
                    DR_NumberROILocation = table.Column<int>(type: "int", nullable: false),
                    L_CornerIndex = table.Column<int>(type: "int", nullable: false),
                    L_DeviceLocationRoi = table.Column<string>(type: "varchar(max)", nullable: true),
                    L_DilationMask = table.Column<int>(type: "int", nullable: false),
                    L_LocationEnable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    L_MinHeightDevice = table.Column<int>(type: "int", nullable: false),
                    L_MinScore = table.Column<double>(type: "double", nullable: false),
                    L_MinWidthDevice = table.Column<int>(type: "int", nullable: false),
                    L_NumberSide = table.Column<int>(type: "int", nullable: false),
                    L_ObjectColor = table.Column<string>(type: "text", nullable: false),
                    L_OpeningMask = table.Column<int>(type: "int", nullable: false),
                    L_ScaleImageRatio = table.Column<double>(type: "double", nullable: false),
                    L_TemplateRoiId = table.Column<int>(type: "int", nullable: true),
                    L_ThresholdType = table.Column<string>(type: "text", nullable: false),
                    L_lowerThreshold = table.Column<int>(type: "int", nullable: false),
                    L_lowerThresholdInnerChip = table.Column<int>(type: "int", nullable: false),
                    L_upperThreshold = table.Column<int>(type: "int", nullable: false),
                    L_upperThresholdInnerChip = table.Column<int>(type: "int", nullable: false),
                    OC_DilationMask = table.Column<int>(type: "int", nullable: false),
                    OC_MinHeightDevice = table.Column<int>(type: "int", nullable: false),
                    OC_MinWidthDevice = table.Column<int>(type: "int", nullable: false),
                    OC_OpeningMask = table.Column<int>(type: "int", nullable: false),
                    OC_lowerThreshold = table.Column<int>(type: "int", nullable: false)
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
    }
}

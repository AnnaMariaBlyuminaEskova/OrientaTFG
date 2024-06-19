using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrientaTFG.Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProfilePictureURLToProfilePictureName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePictureURL",
                schema: "User",
                table: "Tutors",
                newName: "ProfilePictureName");

            migrationBuilder.RenameColumn(
                name: "ProfilePictureURL",
                schema: "User",
                table: "Students",
                newName: "ProfilePictureName");

            migrationBuilder.RenameColumn(
                name: "ProfilePictureURL",
                schema: "User",
                table: "Administrator",
                newName: "ProfilePictureName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePictureName",
                schema: "User",
                table: "Tutors",
                newName: "ProfilePictureURL");

            migrationBuilder.RenameColumn(
                name: "ProfilePictureName",
                schema: "User",
                table: "Students",
                newName: "ProfilePictureURL");

            migrationBuilder.RenameColumn(
                name: "ProfilePictureName",
                schema: "User",
                table: "Administrator",
                newName: "ProfilePictureURL");
        }
    }
}

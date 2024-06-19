﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OrientaTFG.Shared.Infrastructure.Enums;

#nullable disable

namespace OrientaTFG.Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "User");

            migrationBuilder.EnsureSchema(
                name: "Master");

            migrationBuilder.EnsureSchema(
                name: "TFG");

            migrationBuilder.CreateTable(
                name: "Administrator",
                schema: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfilePictureURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LogInRetries = table.Column<int>(type: "int", nullable: false),
                    LogInBlocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrator", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MainTaskStatus",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainTaskStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfilePictureURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LogInRetries = table.Column<int>(type: "int", nullable: false),
                    LogInBlocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubTaskStatus",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubTaskStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tutors",
                schema: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfilePictureURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LogInRetries = table.Column<int>(type: "int", nullable: false),
                    LogInBlocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tutors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tutors_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "Master",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TFG",
                schema: "TFG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TutorId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFG", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TFG_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "User",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TFG_Tutors_TutorId",
                        column: x => x.TutorId,
                        principalSchema: "User",
                        principalTable: "Tutors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MainTasks",
                schema: "TFG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MaximumPoints = table.Column<int>(type: "int", nullable: false),
                    ObtainedPoints = table.Column<int>(type: "int", nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TFGId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainTasks_MainTaskStatus_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Master",
                        principalTable: "MainTaskStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MainTasks_TFG_TFGId",
                        column: x => x.TFGId,
                        principalSchema: "TFG",
                        principalTable: "TFG",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubTasks",
                schema: "TFG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstimatedHours = table.Column<int>(type: "int", nullable: false),
                    TotalHours = table.Column<int>(type: "int", nullable: true),
                    MainTaskId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubTasks_MainTasks_MainTaskId",
                        column: x => x.MainTaskId,
                        principalSchema: "TFG",
                        principalTable: "MainTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubTasks_SubTaskStatus_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Master",
                        principalTable: "SubTaskStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainTasks_StatusId",
                schema: "TFG",
                table: "MainTasks",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MainTasks_TFGId",
                schema: "TFG",
                table: "MainTasks",
                column: "TFGId");

            migrationBuilder.CreateIndex(
                name: "IX_SubTasks_MainTaskId",
                schema: "TFG",
                table: "SubTasks",
                column: "MainTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_SubTasks_StatusId",
                schema: "TFG",
                table: "SubTasks",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TFG_StudentId",
                schema: "TFG",
                table: "TFG",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFG_TutorId",
                schema: "TFG",
                table: "TFG",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tutors_DepartmentId",
                schema: "User",
                table: "Tutors",
                column: "DepartmentId");

            // INSERT DEFAULT DATA DEPARTMENTS
            string[] columnsInsert = ["Id", "Name"];

            migrationBuilder.InsertData(
                table: "Departments",
                columns: columnsInsert,
                values: new object[,]
                {
                    { 1, "Inteligencia Artificial" },
                    { 2, "Software" }
                },
                schema: "Master");

            // INSERT DEFAULT DATA TUTORS
            columnsInsert = ["DepartmentId", "Name", "Surname", "ProfilePictureURL", "Email", "Password", "LogInRetries", "LogInBlocked"];

            migrationBuilder.InsertData(
                table: "Tutors",
                columns: columnsInsert,
                values: new object[,]
                {
                    { 2, "María", "Rodríguez", "url", "maria@gmail.com", "edf9cf90718610ee7de53c0dcc250739239044de9ba115bb0ca6026c3e4958a5", 0, false }
                },
                schema: "User");

            // INSERT DEFAULT DATA STUDENTS
            columnsInsert = ["Name", "Surname", "ProfilePictureURL", "Email", "Password", "LogInRetries", "LogInBlocked"];

            migrationBuilder.InsertData(
                table: "Students",
                columns: columnsInsert,
                values: new object[,]
                {
                    { "Anna", "Blyumina", "url", "annaea0@gmail.com", "edf9cf90718610ee7de53c0dcc250739239044de9ba115bb0ca6026c3e4958a5", 0, false },
                    { "Marcos", "López", "url", "marcos@gmail.com", "524974118f789c395c4be69ba48536de9d2be7b907f078ac0b2fa9ecfb446205", 0, false }
                },
                schema: "User");

            // INSERT DEFAULT DATA MAIN TASK STATUS 
            columnsInsert = ["Id", "Name"];

            migrationBuilder.InsertData(
                table: "MainTaskStatus",
                columns: columnsInsert,
                values: new object[,]
                {
                    { (int)MainTaskStatusEnum.Pendiente, nameof(MainTaskStatusEnum.Pendiente)},
                    { (int)MainTaskStatusEnum.Desarrollo, nameof(MainTaskStatusEnum.Desarrollo) },
                    { (int)MainTaskStatusEnum.Realizado, nameof(MainTaskStatusEnum.Realizado) }
                },
                schema: "Master");

            // INSERT DEFAULT DATA SUB TASK STATUS
            columnsInsert = ["Id", "Name"];

            migrationBuilder.InsertData(
                table: "SubTaskStatus",
                columns: columnsInsert,
                values: new object[,]
                {
                    { (int)SubTaskStatusEnum.Pendiente, nameof(SubTaskStatusEnum.Pendiente) },
                    { (int)SubTaskStatusEnum.Realizado, nameof(SubTaskStatusEnum.Realizado) }
                },
                schema: "Master");

            // INSERT DEFAULT DATA TFG
            columnsInsert = ["Name", "TutorId", "StudentId"];

            migrationBuilder.InsertData(
                table: "TFG",
                columns: columnsInsert,
                values: new object[,]
                {
                    { "TFG Prueba", 1, 2 },
                },
                schema: "TFG");

            // INSERT DEFAULT DATA MAIN TASKS
            columnsInsert = ["Name", "Description", "MaximumPoints", "ObtainedPoints", "Deadline", "TFGId", "StatusId", "Order", "CreatedBy"];

            migrationBuilder.InsertData(
                table: "MainTasks",
                columns: columnsInsert,
                values: new object[,]
                {
                    { "Registro de usuario", "Permitir a los usuarios registrarse en la aplicación", 200, 190,  DateTime.UtcNow, 1, 3, 1, "Tutor"},
                    { "Inicio de sesión", "Permitir a los usuarios autenticarse en la aplicación mediante credenciales", 400, null,  DateTime.UtcNow.AddDays(15), 1, 2, 2, "Tutor"},
                },
                schema: "TFG");

            // INSERT DEFAULT DATA SUB TASKS
            columnsInsert = ["Name", "EstimatedHours", "TotalHours", "MainTaskId", "StatusId", "Order", "CreatedBy"];

            migrationBuilder.InsertData(
                table: "SubTasks",
                columns: columnsInsert,
                values: new object[,]
                {
                    { "Implementación", 8, 10, 1, 2, 1, "Tutor"},
                    { "Tests", 4, 3, 1, 2, 2, "Estudiante"},

                    { "Implementación inicio de sesión", 15, null, 2, 1, 1, "Tutor"},

                },
                schema: "TFG");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrator",
                schema: "User");

            migrationBuilder.DropTable(
                name: "SubTasks",
                schema: "TFG");

            migrationBuilder.DropTable(
                name: "MainTasks",
                schema: "TFG");

            migrationBuilder.DropTable(
                name: "SubTaskStatus",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "MainTaskStatus",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "TFG",
                schema: "TFG");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "User");

            migrationBuilder.DropTable(
                name: "Tutors",
                schema: "User");

            migrationBuilder.DropTable(
                name: "Departments",
                schema: "Master");
        }
    }
}

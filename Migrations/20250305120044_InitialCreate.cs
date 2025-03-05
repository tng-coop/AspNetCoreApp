using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AspNetCoreApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "09e20cb8-fa0a-43f8-b746-bb9d7f38af59", 0, "4c0939f8-574b-4b99-9a4e-27c502494e8d", "philip@example.com", true, false, null, "PHILIP@EXAMPLE.COM", "PHILIP@EXAMPLE.COM", null, null, false, "d22783ff-3d12-4d24-8e3e-1b4125017d66", false, "philip@example.com" },
                    { "0fd088af-ea18-47a5-be6a-8de9d122b000", 0, "1d68c85a-ec35-4ca9-b776-123a16875598", "simon.peter@example.com", true, false, null, "SIMON.PETER@EXAMPLE.COM", "SIMON.PETER@EXAMPLE.COM", null, null, false, "4103531e-e81b-4a8f-a041-ddbc847762da", false, "simon.peter@example.com" },
                    { "30b33dfc-7907-4c59-80c4-c377befa8c17", 0, "49069f8c-43bc-4c55-abb1-1252ea17509e", "matthias@example.com", true, false, null, "MATTHIAS@EXAMPLE.COM", "MATTHIAS@EXAMPLE.COM", null, null, false, "19baadce-ca91-4d3e-9a3c-311ceb4b60a0", false, "matthias@example.com" },
                    { "5200e26a-1ebe-4494-a268-d15f79335494", 0, "a6e4af6c-34bb-4bea-9d4a-052faeee7b35", "bartholomew@example.com", true, false, null, "BARTHOLOMEW@EXAMPLE.COM", "BARTHOLOMEW@EXAMPLE.COM", null, null, false, "1bb772a6-eb4f-456a-bc75-406cf27cfb10", false, "bartholomew@example.com" },
                    { "53c267ed-3802-41cf-9fd5-cf30a6a1784a", 0, "064b86f2-09a8-4be1-a532-80119c6dd33b", "thomas@example.com", true, false, null, "THOMAS@EXAMPLE.COM", "THOMAS@EXAMPLE.COM", null, null, false, "39e91aef-a592-4301-854a-97fe07d253f7", false, "thomas@example.com" },
                    { "6d1d973a-0da6-44f0-9cee-d3a7b68e592c", 0, "17c0ba9c-74b1-4d70-82a0-f24a6289679b", "judas.iscariot@example.com", true, false, null, "JUDAS.ISCARIOT@EXAMPLE.COM", "JUDAS.ISCARIOT@EXAMPLE.COM", null, null, false, "d0a32f1b-bb31-41c9-a6dd-ef228ef48391", false, "judas.iscariot@example.com" },
                    { "94bb6083-cfbb-4b48-ab12-7771ba0c08c0", 0, "d9ed4d11-bd63-4fa9-8183-18b531dbde3c", "paul@example.com", true, false, null, "PAUL@EXAMPLE.COM", "PAUL@EXAMPLE.COM", null, null, false, "68c72e71-7dae-4314-bbbc-0a68652292b9", false, "paul@example.com" },
                    { "9ad78183-057c-4553-b94a-fdfc403284e1", 0, "7e123711-7fee-4f41-a338-8d9c3419157d", "simon.zealot@example.com", true, false, null, "SIMON.ZEALOT@EXAMPLE.COM", "SIMON.ZEALOT@EXAMPLE.COM", null, null, false, "74ee57c6-a245-4e54-976f-ddf4692acce7", false, "simon.zealot@example.com" },
                    { "a15a9cf0-c043-4c7d-b3e8-8bf14bba45b0", 0, "3dfbfa7d-adda-4ca6-be09-10e8127b9ebc", "thaddaeus@example.com", true, false, null, "THADDAEUS@EXAMPLE.COM", "THADDAEUS@EXAMPLE.COM", null, null, false, "fd1a2d1e-63f0-4e4a-a541-9a3a4c2c6c67", false, "thaddaeus@example.com" },
                    { "c04109ba-6626-4e74-9b03-68eb94b9cbe2", 0, "17a966b4-1550-46cc-bf3e-d16c2361a98d", "andrew@example.com", true, false, null, "ANDREW@EXAMPLE.COM", "ANDREW@EXAMPLE.COM", null, null, false, "4717b963-0011-4bb9-9582-f1fea5c1f8f0", false, "andrew@example.com" },
                    { "c25fe4db-62b8-4bbd-a151-f65f6673ee14", 0, "4eb3cb85-5462-4e3f-9a00-4c957e0e6843", "matthew.levi@example.com", true, false, null, "MATTHEW.LEVI@EXAMPLE.COM", "MATTHEW.LEVI@EXAMPLE.COM", null, null, false, "18115745-3e4b-4f34-a787-bda8db791527", false, "matthew.levi@example.com" },
                    { "d317aa03-8531-43e2-9d26-15e05e5a3966", 0, "f5accac3-df96-49bf-b850-ad307a346de2", "james.zebedee@example.com", true, false, null, "JAMES.ZEBEDEE@EXAMPLE.COM", "JAMES.ZEBEDEE@EXAMPLE.COM", null, null, false, "56ec27ea-4385-4fa7-a268-12abafefa077", false, "james.zebedee@example.com" },
                    { "ee237298-78ec-48ef-ac6f-878e6f42d676", 0, "db8f652a-abec-4776-bb08-c4f157c816bd", "james.alphaeus@example.com", true, false, null, "JAMES.ALPHAEUS@EXAMPLE.COM", "JAMES.ALPHAEUS@EXAMPLE.COM", null, null, false, "859f9772-91e7-4b80-8b95-55ae1b216915", false, "james.alphaeus@example.com" },
                    { "f1a37d8f-959c-40e5-b84d-f2605b66a99f", 0, "ed60b3b1-59ad-4eed-a6a9-25d943f01305", "john.zebedee@example.com", true, false, null, "JOHN.ZEBEDEE@EXAMPLE.COM", "JOHN.ZEBEDEE@EXAMPLE.COM", null, null, false, "52fd3b61-d7d4-4734-98f3-0dfc2772ea10", false, "john.zebedee@example.com" }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "Email", "FirstName", "JoinedDate", "LastName" },
                values: new object[,]
                {
                    { 1, "simon.peter@example.com", "Simon", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4891), "Peter" },
                    { 2, "andrew@example.com", "Andrew", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4892), "" },
                    { 3, "james.zebedee@example.com", "James", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4893), "son of Zebedee" },
                    { 4, "john.zebedee@example.com", "John", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4894), "son of Zebedee" },
                    { 5, "philip@example.com", "Philip", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4895), "" },
                    { 6, "bartholomew@example.com", "Bartholomew", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4896), "" },
                    { 7, "thomas@example.com", "Thomas", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4897), "" },
                    { 8, "matthew.levi@example.com", "Matthew", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(4898), "Levi" },
                    { 9, "james.alphaeus@example.com", "James", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(5020), "son of Alphaeus" },
                    { 10, "thaddaeus@example.com", "Thaddaeus", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(5021), "" },
                    { 11, "simon.zealot@example.com", "Simon", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(5022), "the Zealot" },
                    { 12, "judas.iscariot@example.com", "Judas", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(5023), "Iscariot" },
                    { 13, "matthias@example.com", "Matthias", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(5024), "" },
                    { 14, "paul@example.com", "Paul", new DateTime(2025, 3, 5, 12, 0, 43, 945, DateTimeKind.Utc).AddTicks(5025), "" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}

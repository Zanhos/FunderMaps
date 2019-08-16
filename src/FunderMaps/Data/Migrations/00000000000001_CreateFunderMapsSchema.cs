﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using FunderMaps.Extensions;

namespace FunderMaps.Data.Migrations
{
    public partial class CreateFunderMapsSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureExtension("uuid-ossp");

            migrationBuilder.EnsureSchema("application");

            migrationBuilder.CreateSequence(
                name: "address_id_seq",
                schema: "application");

            migrationBuilder.CreateTable(
                name: "address",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, name: "id", defaultValueSql: "nextval('application.address_id_seq'::regclass)"),
                    Address = table.Column<string>(maxLength: 256, nullable: false, name: "address"),
                    AddressNumber = table.Column<int>(nullable: false, name: "address_number"),
                    AddressNumberPostfix = table.Column<string>(maxLength: 8, nullable: true, name: "address_number_postfix"),
                    City = table.Column<string>(maxLength: 256, nullable: true, name: "city"),
                    Postbox = table.Column<string>(maxLength: 8, nullable: true, name: "postbox"),
                    Zipcode = table.Column<string>(maxLength: 8, nullable: true, name: "zipcode"),
                    State = table.Column<string>(maxLength: 256, nullable: true, name: "state"),
                    Country = table.Column<string>(maxLength: 256, nullable: true, name: "country"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address", x => x.Id);
                });

            migrationBuilder.CreateTable(
               name: "organization_role",
               schema: "application",
               columns: table => new
               {
                   Id = table.Column<Guid>(nullable: false, name: "id", defaultValueSql: "uuid_generate_v4()"),
                   Name = table.Column<string>(maxLength: 256, nullable: true, name: "name"),
                   NormalizedName = table.Column<string>(maxLength: 256, nullable: true, name: "normalized_name"),
                   ConcurrencyStamp = table.Column<string>(nullable: true, name: "concurrency_stamp")
               },
               constraints: table =>
               {
                   table.PrimaryKey("pk_organization_role", x => x.Id);
               });

            migrationBuilder.CreateTable(
                name: "organization",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, name: "id", defaultValueSql: "uuid_generate_v4()"),
                    Name = table.Column<string>(maxLength: 256, nullable: false, name: "name"),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: false, name: "normalized_name"),
                    Email = table.Column<string>(maxLength: 256, name: "email"),
                    PhoneNumber = table.Column<string>(nullable: true, name: "phone_number"),
                    HomeAddressId = table.Column<int>(nullable: false, name: "home_address_id"),
                    PostalAddressId = table.Column<int>(nullable: false, name: "postal_address_id"),
                    RegistrationNumber = table.Column<string>(maxLength: 40, nullable: true, name: "registration_number"),
                    IsDefault = table.Column<bool>(nullable: false, defaultValue: false, name: "is_default"),
                    IsValidated = table.Column<bool>(nullable: false, defaultValue: false, name: "is_validated"),
                    BrandingLogo = table.Column<string>(nullable: true, maxLength: 256, name: "branding_logo"),
                    InvoiceName = table.Column<string>(nullable: true, maxLength: 256, name: "invoice_name"),
                    InvoicePONumber = table.Column<string>(nullable: true, maxLength: 256, name: "invoice_po_number"),
                    InvoiceEmail = table.Column<string>(nullable: true, maxLength: 256, name: "invoice_email"),
                    AttestationOrganizationId = table.Column<int>(nullable: true, name: "attestation_organization_id")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization", x => x.Id);
                    table.ForeignKey(
                        name: "fk_address_home_address_id",
                        column: x => x.HomeAddressId,
                        principalSchema: "application",
                        principalTable: "address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_address_postal_address_id",
                        column: x => x.PostalAddressId,
                        principalSchema: "application",
                        principalTable: "address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_user",
                schema: "application",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false, name: "user_id"),
                    OrganizationId = table.Column<Guid>(nullable: false, name: "organization_id"),
                    OrganizationRoleId = table.Column<Guid>(nullable: false, name: "organization_role_id")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_user", x => new { x.UserId, x.OrganizationId, x.OrganizationRoleId });
                    table.ForeignKey(
                        name: "fk_organization_user_user_id",
                        column: x => x.UserId,
                        principalSchema: "application",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_user_organization_id",
                        column: x => x.OrganizationId,
                        principalSchema: "application",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_user_organization_role_id",
                        column: x => x.OrganizationRoleId,
                        principalSchema: "application",
                        principalTable: "organization_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_proposal",
                schema: "application",
                columns: table => new
                {
                    Token = table.Column<Guid>(nullable: false, name: "token", defaultValueSql: "uuid_generate_v4()"),
                    Name = table.Column<string>(maxLength: 256, nullable: false, name: "name"),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: false, name: "normalized_name"),
                    Email = table.Column<string>(maxLength: 256, nullable: false, name: "email"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_proposal", x => x.Token);
                });

            migrationBuilder.CreateIndex(
                name: "idx_organization_role_normalized_name",
                schema: "application",
                table: "organization_role",
                column: "normalized_name",
                unique: true,
                filter: "\"normalized_name\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_organization_normalized_name",
                schema: "application",
                table: "organization",
                column: "normalized_name",
                unique: true,
                filter: "\"normalized_name\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_organization_proposal_normalized_name",
                schema: "application",
                table: "organization_proposal",
                column: "normalized_name",
                unique: true,
                filter: "\"normalized_name\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("organization_proposal", "application");

            migrationBuilder.DropTable("organization_user", "application");

            migrationBuilder.DropTable("organization", "application");

            migrationBuilder.DropTable("organization_role", "application");

            migrationBuilder.DropTable("address", "application");

            migrationBuilder.DropSequence("address_id_seq", "application");

            migrationBuilder.DropSchema("application");

            migrationBuilder.DropExtension("uuid-ossp");
        }
    }
}
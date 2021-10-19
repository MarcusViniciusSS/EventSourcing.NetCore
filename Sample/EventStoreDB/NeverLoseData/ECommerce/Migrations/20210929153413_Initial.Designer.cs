﻿// <auto-generated />
using System;
using ECommerce.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ECommerce.Migrations
{
    [DbContext(typeof(ECommerceDbContext))]
    [Migration("20210929153413_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("ECommerce.ShoppingCarts.GettingCartById.ShoppingCartDetails", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Version")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ShoppingCartDetails");
                });

            modelBuilder.Entity("ECommerce.ShoppingCarts.GettingCarts.ShoppingCartShortInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("TotalItemsCount")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("numeric");

                    b.Property<int>("Version")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ShoppingCartShortInfo");
                });

            modelBuilder.Entity("ECommerce.ShoppingCarts.GettingCartById.ShoppingCartDetails", b =>
                {
                    b.OwnsMany("ECommerce.ShoppingCarts.GettingCartById.ShoppingCartDetailsProductItem", "ProductItems", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                            b1.Property<Guid>("ProductId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Quantity")
                                .HasColumnType("integer");

                            b1.Property<Guid>("ShoppingCardId")
                                .HasColumnType("uuid");

                            b1.Property<decimal>("UnitPrice")
                                .HasColumnType("numeric");

                            b1.HasKey("Id");

                            b1.HasIndex("ShoppingCardId");

                            b1.ToTable("ShoppingCartDetailsProductItem");

                            b1.WithOwner()
                                .HasForeignKey("ShoppingCardId");
                        });

                    b.Navigation("ProductItems");
                });
#pragma warning restore 612, 618
        }
    }
}

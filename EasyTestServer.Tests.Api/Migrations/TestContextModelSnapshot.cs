﻿// <auto-generated />
using System;
using EasyTestServer.Tests.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EasyTestServer.Tests.Api.Migrations
{
    [DbContext(typeof(UserContext))]
    partial class TestContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("EasyTestServer.Tests.Api.Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("EasyTestServer.Tests.Api.Domain.User", b =>
                {
                    b.OwnsMany("EasyTestServer.Tests.Api.Domain.Friend", "Friends", b1 =>
                        {
                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("UserId")
                                .HasColumnType("TEXT");

                            b1.HasKey("Id");

                            b1.HasIndex("UserId");

                            b1.ToTable("Friend");

                            b1.WithOwner()
                                .HasForeignKey("UserId");
                        });

                    b.Navigation("Friends");
                });
#pragma warning restore 612, 618
        }
    }
}

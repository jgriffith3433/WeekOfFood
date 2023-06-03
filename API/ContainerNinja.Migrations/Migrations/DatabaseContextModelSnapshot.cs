﻿// <auto-generated />
using System;
using ContainerNinja.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CalledIngredient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ProductStockId")
                        .HasColumnType("int");

                    b.Property<int>("RecipeId")
                        .HasColumnType("int");

                    b.Property<int>("UnitType")
                        .HasColumnType("int");

                    b.Property<float?>("Units")
                        .HasColumnType("real");

                    b.Property<bool>("Verified")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("ProductStockId");

                    b.HasIndex("RecipeId");

                    b.ToTable("CalledIngredient");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.ChatCommand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("ChangedData")
                        .HasColumnType("bit");

                    b.Property<int>("ChatConversationId")
                        .HasColumnType("int");

                    b.Property<string>("CommandName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NavigateToPage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RawChatAICommand")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("UnknownCommand")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("ChatConversationId");

                    b.ToTable("ChatCommands");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.ChatConversation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ChatConversations");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CompletedOrder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserImport")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CompletedOrders");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CompletedOrderProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CompletedOrderId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int");

                    b.Property<string>("WalmartError")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("WalmartId")
                        .HasColumnType("bigint");

                    b.Property<string>("WalmartItemResponse")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WalmartSearchResponse")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CompletedOrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("CompletedOrderProducts");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CookedRecipe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RecipeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("CookedRecipes");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CookedRecipeCalledIngredient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CalledIngredientId")
                        .HasColumnType("int");

                    b.Property<int>("CookedRecipeId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ProductStockId")
                        .HasColumnType("int");

                    b.Property<int>("UnitType")
                        .HasColumnType("int");

                    b.Property<float?>("Units")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("CalledIngredientId");

                    b.HasIndex("CookedRecipeId");

                    b.HasIndex("ProductStockId");

                    b.ToTable("CookedRecipeCalledIngredients");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.Item", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Categories")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ColorCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("Price")
                        .HasColumnType("real");

                    b.Property<float>("Size")
                        .HasColumnType("real");

                    b.Property<int>("UnitType")
                        .HasColumnType("int");

                    b.Property<bool>("Verified")
                        .HasColumnType("bit");

                    b.Property<long?>("WalmartId")
                        .HasColumnType("bigint");

                    b.Property<string>("WalmartItemResponse")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WalmartLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WalmartSearchResponse")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WalmartSize")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.ProductStock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<float?>("Units")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("ProductId")
                        .IsUnique();

                    b.ToTable("ProductStocks");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.Recipe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Link")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Serves")
                        .HasColumnType("int");

                    b.Property<string>("UserImport")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.TodoList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("TodoLists");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CalledIngredient", b =>
                {
                    b.HasOne("ContainerNinja.Contracts.Data.Entities.ProductStock", "ProductStock")
                        .WithMany()
                        .HasForeignKey("ProductStockId");

                    b.HasOne("ContainerNinja.Contracts.Data.Entities.Recipe", "Recipe")
                        .WithMany("CalledIngredients")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProductStock");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.ChatCommand", b =>
                {
                    b.HasOne("ContainerNinja.Contracts.Data.Entities.ChatConversation", null)
                        .WithMany("ChatCommands")
                        .HasForeignKey("ChatConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CompletedOrderProduct", b =>
                {
                    b.HasOne("ContainerNinja.Contracts.Data.Entities.CompletedOrder", "CompletedOrder")
                        .WithMany("CompletedOrderProducts")
                        .HasForeignKey("CompletedOrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ContainerNinja.Contracts.Data.Entities.Product", "Product")
                        .WithMany("CompletedOrderProducts")
                        .HasForeignKey("ProductId");

                    b.Navigation("CompletedOrder");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CookedRecipe", b =>
                {
                    b.HasOne("ContainerNinja.Contracts.Data.Entities.Recipe", "Recipe")
                        .WithMany("CookedRecipes")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CookedRecipeCalledIngredient", b =>
                {
                    b.HasOne("ContainerNinja.Contracts.Data.Entities.CalledIngredient", "CalledIngredient")
                        .WithMany()
                        .HasForeignKey("CalledIngredientId");

                    b.HasOne("ContainerNinja.Contracts.Data.Entities.CookedRecipe", "CookedRecipe")
                        .WithMany("CookedRecipeCalledIngredients")
                        .HasForeignKey("CookedRecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ContainerNinja.Contracts.Data.Entities.ProductStock", "ProductStock")
                        .WithMany()
                        .HasForeignKey("ProductStockId");

                    b.Navigation("CalledIngredient");

                    b.Navigation("CookedRecipe");

                    b.Navigation("ProductStock");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.ProductStock", b =>
                {
                    b.HasOne("ContainerNinja.Contracts.Data.Entities.Product", "Product")
                        .WithOne("ProductStock")
                        .HasForeignKey("ContainerNinja.Contracts.Data.Entities.ProductStock", "ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.ChatConversation", b =>
                {
                    b.Navigation("ChatCommands");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CompletedOrder", b =>
                {
                    b.Navigation("CompletedOrderProducts");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.CookedRecipe", b =>
                {
                    b.Navigation("CookedRecipeCalledIngredients");
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.Product", b =>
                {
                    b.Navigation("CompletedOrderProducts");

                    b.Navigation("ProductStock")
                        .IsRequired();
                });

            modelBuilder.Entity("ContainerNinja.Contracts.Data.Entities.Recipe", b =>
                {
                    b.Navigation("CalledIngredients");

                    b.Navigation("CookedRecipes");
                });
#pragma warning restore 612, 618
        }
    }
}

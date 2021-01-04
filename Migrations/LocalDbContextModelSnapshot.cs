﻿// <auto-generated />
using System;
using CESystem.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CESystem.Migrations
{
    [DbContext(typeof(LocalDbContext))]
    partial class LocalDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("CESystem.Models.AccountRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("id_user");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("account");
                });

            modelBuilder.Entity("CESystem.Models.CommissionRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<int?>("CurrencyId")
                        .HasColumnType("integer")
                        .HasColumnName("id_currency");

                    b.Property<float?>("DepositCommission")
                        .HasColumnType("real")
                        .HasColumnName("deposit");

                    b.Property<bool?>("IsAbsoluteType")
                        .HasColumnType("boolean")
                        .HasColumnName("is_absolute_type");

                    b.Property<float?>("TransferCommission")
                        .HasColumnType("real")
                        .HasColumnName("transfer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("id_user");

                    b.Property<float?>("WithdrawCommission")
                        .HasColumnType("real")
                        .HasColumnName("withdraw");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId")
                        .IsUnique();

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("commission");

                    b.HasCheckConstraint("correct_commission", "'id_user' != null or 'id_currency' != null");
                });

            modelBuilder.Entity("CESystem.Models.ConfirmRequestRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<float>("Amount")
                        .HasColumnType("real")
                        .HasColumnName("amount");

                    b.Property<float>("Commission")
                        .HasColumnType("real")
                        .HasColumnName("commission");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency");

                    b.Property<string>("FormationDate")
                        .HasColumnType("text")
                        .HasColumnName("formation_date");

                    b.Property<int>("OperationType")
                        .HasColumnType("integer")
                        .HasColumnName("operation_type");

                    b.Property<int?>("RecipientId")
                        .HasColumnType("integer")
                        .HasColumnName("id_recipient");

                    b.Property<int>("SenderId")
                        .HasColumnType("integer")
                        .HasColumnName("id_sender");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("SenderId");

                    b.ToTable("confirm_request");
                });

            modelBuilder.Entity("CESystem.Models.CurrencyRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<float?>("ConfirmCommissionLimit")
                        .HasColumnType("real")
                        .HasColumnName("confirm_limit");

                    b.Property<float?>("LowerCommissionLimit")
                        .HasColumnType("real")
                        .HasColumnName("lower_commission_limit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<float?>("UpperCommissionLimit")
                        .HasColumnType("real")
                        .HasColumnName("upper_commission_limit");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("currency");
                });

            modelBuilder.Entity("CESystem.Models.OperationsHistoryRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("AccountId")
                        .HasColumnType("integer")
                        .HasColumnName("id_account");

                    b.Property<float>("Commission")
                        .HasColumnType("real")
                        .HasColumnName("commission");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency");

                    b.Property<string>("Date")
                        .HasColumnType("text")
                        .HasColumnName("date");

                    b.Property<float>("Sum")
                        .HasColumnType("real")
                        .HasColumnName("amount");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("id_user");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("operations_history");
                });

            modelBuilder.Entity("CESystem.Models.UserRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("CreatedDate")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_date");

                    b.Property<int>("CurrentAccount")
                        .HasColumnType("integer")
                        .HasColumnName("current_account");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password_hash");

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password_salt");

                    b.Property<string>("Role")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("client")
                        .HasColumnName("role");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("users");
                });

            modelBuilder.Entity("CESystem.Models.WalletRecord", b =>
                {
                    b.Property<int>("IdAccount")
                        .HasColumnType("integer")
                        .HasColumnName("id_account");

                    b.Property<int>("IdCurrency")
                        .HasColumnType("integer")
                        .HasColumnName("id_currency");

                    b.Property<float>("CashValue")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f)
                        .HasColumnName("cash_value");

                    b.HasKey("IdAccount", "IdCurrency");

                    b.HasIndex("IdCurrency");

                    b.ToTable("wallet");
                });

            modelBuilder.Entity("CESystem.Models.AccountRecord", b =>
                {
                    b.HasOne("CESystem.Models.UserRecord", "UserRecord")
                        .WithMany("AccountRecords")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_id_user_account")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserRecord");
                });

            modelBuilder.Entity("CESystem.Models.CommissionRecord", b =>
                {
                    b.HasOne("CESystem.Models.CurrencyRecord", "CurrencyRecord")
                        .WithOne("CommissionRecord")
                        .HasForeignKey("CESystem.Models.CommissionRecord", "CurrencyId")
                        .HasConstraintName("FK_id_currency_commission");

                    b.HasOne("CESystem.Models.UserRecord", "UserRecord")
                        .WithOne("CommissionRecord")
                        .HasForeignKey("CESystem.Models.CommissionRecord", "UserId")
                        .HasConstraintName("FK_id_user_commission");

                    b.Navigation("CurrencyRecord");

                    b.Navigation("UserRecord");
                });

            modelBuilder.Entity("CESystem.Models.ConfirmRequestRecord", b =>
                {
                    b.HasOne("CESystem.Models.AccountRecord", "SenderAccountRecord")
                        .WithMany("ConfirmRequestRecords")
                        .HasForeignKey("SenderId")
                        .HasConstraintName("FK_id_account_confirm_request")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SenderAccountRecord");
                });

            modelBuilder.Entity("CESystem.Models.OperationsHistoryRecord", b =>
                {
                    b.HasOne("CESystem.Models.UserRecord", "UserRecord")
                        .WithMany("OperationsHistoryRecords")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_id_user_history")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserRecord");
                });

            modelBuilder.Entity("CESystem.Models.WalletRecord", b =>
                {
                    b.HasOne("CESystem.Models.AccountRecord", "AccountRecord")
                        .WithMany("WalletRecords")
                        .HasForeignKey("IdAccount")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CESystem.Models.CurrencyRecord", "CurrencyRecord")
                        .WithMany("WalletRecords")
                        .HasForeignKey("IdCurrency")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AccountRecord");

                    b.Navigation("CurrencyRecord");
                });

            modelBuilder.Entity("CESystem.Models.AccountRecord", b =>
                {
                    b.Navigation("ConfirmRequestRecords");

                    b.Navigation("WalletRecords");
                });

            modelBuilder.Entity("CESystem.Models.CurrencyRecord", b =>
                {
                    b.Navigation("CommissionRecord");

                    b.Navigation("WalletRecords");
                });

            modelBuilder.Entity("CESystem.Models.UserRecord", b =>
                {
                    b.Navigation("AccountRecords");

                    b.Navigation("CommissionRecord");

                    b.Navigation("OperationsHistoryRecords");
                });
#pragma warning restore 612, 618
        }
    }
}

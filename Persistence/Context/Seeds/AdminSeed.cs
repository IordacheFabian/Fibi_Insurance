using System;
using Domain.Models.AppUsers;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context.Seeds;

public static class AdminSeed
{
	public static readonly Guid DefaultAdminId =
		Guid.Parse("8f5f4a82-6e4d-4de3-9f7d-6fb52e1d3e10");

	public const string DefaultAdminEmail = "admin@insurance.local";

	private const string DefaultAdminRole = "Admin";
	private const string DefaultAdminPassword = "Admin123!";
	private const string DefaultAdminPasswordSalt = "$2a$11$abcdefghijklmnopqrstuu";

	public static void Seed(ModelBuilder builder)
	{
		builder.Entity<AppUser>().HasData(
			new AppUser
			{
				Id = DefaultAdminId,
				Email = DefaultAdminEmail,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword, DefaultAdminPasswordSalt),
				Role = DefaultAdminRole,
				BrokerId = null,
				isActive = true,
				CreatedAt = new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc),
				UpdatedAt = null
			}
		);
	}
}

using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Enums; 
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Seeds;

public class WorkSpaceSeeder(WorkSpaceContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : IWorkSpaceSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {

        await context.Database.MigrateAsync(ct);

        if (await roleManager.Roles.AnyAsync(ct)) return;

        var rolesToSeed = new (string Name, string DisplayName)[]
        {
            (Roles.Admin.ToString(), "Administrator"), 
            (Roles.Staff.ToString(), "Staff"),         
            (Roles.Owner.ToString(), "Owner"),         
            (Roles.Customer.ToString(), "Customer")    
        };

        foreach (var (name, displayName) in rolesToSeed)
        {
            await CreateRoleIfNotExistsAsync(name, displayName, ct);
        }

   
        const string adminEmail = "admin@workspace.local"; 
        const string adminPass = "Password1!";
        await EnsureUserWithRolesAsync(
            email: adminEmail,
            password: adminPass,
            firstName: "System", 
            lastName: "Admin",
            roles: new[] { Roles.Admin.ToString() }, 
            ct: ct
        );

     
        await SeedBookingStatusesAsync(ct);

    }


    private async Task CreateRoleIfNotExistsAsync(string roleName, string displayName, CancellationToken ct)
    {
        if (await roleManager.RoleExistsAsync(roleName)) return;

        var role = new AppRole
        {
            Name = roleName,
            DisplayName = displayName 
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new InvalidOperationException($"Create role '{roleName}' failed: {errors}");
        }
    }

    private async Task EnsureUserWithRolesAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        IEnumerable<string> roles,
        CancellationToken ct = default
    )
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email, 
                Email = email,
                EmailConfirmed = true, 
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            var createRes = await userManager.CreateAsync(user, password);
            if (!createRes.Succeeded)
            {
                var errors = string.Join("; ", createRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Create user '{email}' failed: {errors}");
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var toAdd = roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();
        if (toAdd.Length > 0)
        {
            var addRes = await userManager.AddToRolesAsync(user, toAdd);
            if (!addRes.Succeeded)
            {
                var errors = string.Join("; ", addRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Add roles '{string.Join(",", toAdd)}' to '{email}' failed: {errors}");
            }
        }
    }

    private async Task SeedBookingStatusesAsync(CancellationToken ct)
    {
        if (await context.BookingStatuses.AnyAsync(ct)) return;

        var bookingStatuses = new[]
        {
            new BookingStatus { Name = "Pending", Description = "Booking is awaiting confirmation or payment", CreateUtc = DateTimeOffset.UtcNow },
            new BookingStatus { Name = "Confirmed", Description = "Booking has been confirmed and paid", CreateUtc = DateTimeOffset.UtcNow },
            new BookingStatus { Name = "InProgress", Description = "Booking is currently in progress", CreateUtc = DateTimeOffset.UtcNow },
            new BookingStatus { Name = "Completed", Description = "Booking has been completed successfully", CreateUtc = DateTimeOffset.UtcNow },
            new BookingStatus { Name = "Cancelled", Description = "Booking has been cancelled", CreateUtc = DateTimeOffset.UtcNow },
            new BookingStatus { Name = "Expired", Description = "Booking has expired without confirmation", CreateUtc = DateTimeOffset.UtcNow },
            new BookingStatus { Name = "NoShow", Description = "Customer did not show up for confirmed booking", CreateUtc = DateTimeOffset.UtcNow },
            new BookingStatus { Name = "Refunded", Description = "Booking has been cancelled and refunded", CreateUtc = DateTimeOffset.UtcNow }
        };

        await context.BookingStatuses.AddRangeAsync(bookingStatuses, ct);
        await context.SaveChangesAsync(ct);
    }
}
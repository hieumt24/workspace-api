using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using VNPAY.NET;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.ConfigOptions;
using WorkSpace.Domain.Entities;
using WorkSpace.Infrastructure.Repositories;
using WorkSpace.Infrastructure.Seeds;
using WorkSpace.Infrastructure.Services;

namespace WorkSpace.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();

        #region Repositories
        services.AddScoped(typeof(IWorkSpaceRepository), typeof(WorkSpaceRepository));
        services.AddTransient(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
        services.AddScoped(typeof(IHostProfileAsyncRepository), typeof(HostProfileAsyncProfileAsyncRepository));
        services.AddScoped(typeof(IWorkSpaceFavoriteRepository), typeof(WorkSpaceFavoriteRepository));
        services.AddScoped(typeof(IPromotionRepository), typeof(PromotionRepository));
        services.AddScoped(typeof(IBookingStatusRepository), typeof(BookingStatusRepository));
        services.AddScoped(typeof(IWorkSpaceTypeRepository), typeof(WorkSpaceTypeRepository));
        services.AddScoped(typeof(IBookingRepository), typeof(BookingRepository));
        services.AddScoped(typeof(IGuestRepository), typeof(GuestRepository));
        services.AddScoped<IVnpay, Vnpay>();
        services.AddScoped(typeof(IBlockedTimeSlotRepository), typeof(BlockedTimeSlotRepository));
        services.AddScoped(typeof(IPostRepository), typeof(PostRepository));
        services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
        services.AddScoped(typeof(IChatMessageRepository), typeof(ChatMessageRepository));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<WorkSpaceContext>());
        #endregion

        #region Services
        services.AddScoped<IWorkSpaceSeeder, WorkSpaceSeeder>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IVNPayService, VNPayService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddHttpContextAccessor();
        services.AddScoped(typeof(IRecommendationService), typeof(RecommendationService));
        services.AddScoped(typeof(IAIChatbotService), typeof(AIChatbotService));
        #endregion

        #region Identity
        services.AddIdentityCore<AppUser>()
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<WorkSpaceContext>()
            .AddDefaultTokenProviders();
        #endregion
        
        #region Configuration
        services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
        services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
        services.Configure<VNPaySettings>(configuration.GetSection("VNPaySettings"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));
        #endregion
        
        #region Authentication

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = false;

                    o.MapInboundClaims = false; 
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = configuration["JWTSettings:Issuer"],
                        ValidAudience = configuration["JWTSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"])),
                        RoleClaimType = "role"
                    };
                    o.Events = new JwtBearerEvents()
                    {
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            return c.Response.WriteAsync(c.Exception.ToString());
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(new Response<string>("You are not Authorized"));
                            return context.Response.WriteAsync(result);
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(new Response<string>("You are not authorized to access this resource"));
                            return context.Response.WriteAsync(result);
                        },
                    };
                });
        #endregion
    }
}
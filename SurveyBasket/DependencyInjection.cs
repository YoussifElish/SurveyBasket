using FluentValidation.AspNetCore;
using Hangfire;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Authentication;
using SurveyBasket.Health;
using SurveyBasket.Settings;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

namespace SurveyBasket
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Add services to the container.
            services.AddControllers();
            services.AddHybridCache();

            // Identity Configuration
            services.AddIdentityConfig();

            // CORS Configuration
            services.AddCors(options =>
                options.AddDefaultPolicy(builder =>
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>())
                )
            );

            // Add Database Connection
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection String Not Found");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            // Add Configurations
            services.AddSwaggerConfig()
                    .AddMapsterConfig()
                    .AddFluentValidationConfig()
                    .AddAuthConfig(configuration);

            // Scoped Services

            services.AddScoped<IPollService, PollService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IVoteService, VoteService>();
            services.AddScoped<IResultService, ResultService>();
            services.AddScoped<IEmailSender, EmailService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddHttpContextAccessor();

            services.AddBackgroundJobsConfig(configuration);

            // Singleton Services
            services.AddSingleton<IJwtProvider, JwtProvider>();

            // Exception Handling and Problem Details
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
            services.AddHealthChecks().AddSqlServer(name: "Database", connectionString: configuration.GetConnectionString("DefaultConnection")!, tags: ["Database"]).AddHangfire(options => { options.MinimumAvailableServers = 1; }, tags: ["HangFire"]).AddCheck<MailProviderHealthCheck>(name: "Mail Service", tags: ["Mail Service"]);

            services.AddRateLimiter(rateLimiterOptions =>
            {
                rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                rateLimiterOptions.AddPolicy("ipLimit", httpContent =>
                    RateLimitPartition.GetFixedWindowLimiter(
                       partitionKey: httpContent.Connection.RemoteIpAddress?.ToString(),
                       factory: _ => new FixedWindowRateLimiterOptions
                       {
                           PermitLimit = 2,
                           Window = TimeSpan.FromSeconds(20)
                       }
                        )
                );



                rateLimiterOptions.AddPolicy("userLimit", httpContent =>
                  RateLimitPartition.GetFixedWindowLimiter(
                     partitionKey: httpContent.User.GetUserId(),
                     factory: _ => new FixedWindowRateLimiterOptions
                     {
                         PermitLimit = 2,
                         Window = TimeSpan.FromSeconds(20)
                     }
                      )
              );
                rateLimiterOptions.AddConcurrencyLimiter("concurrency", options =>
                {
                    options.PermitLimit = 1000;
                    options.QueueLimit = 100;
                    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                });
            });


            return services;
        }

        // Swagger Configuration
        private static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        // Mapster Configuration
        private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
        {
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(mappingConfig));

            return services;
        }

        // FluentValidation Configuration
        private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation()
                    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }

        // JWT and Identity Configuration
        private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                    .BindConfiguration("Jwt")
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            var jwtSettings = configuration.GetSection("Jwt").Get<JwtOptions>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience
                };
            });

            return services;
        }

        // Identity Configuration
        private static IServiceCollection AddIdentityConfig(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;

            });

            return services;
        }
        private static IServiceCollection AddBackgroundJobsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Hangfire services.
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

            services.AddHangfireServer();


            return services;
        }




    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Authentication.Data;
using Authentication.Options;
using System.Text;

namespace Authentication.Extensions
{
	public static class WebApplicationBuilderExtension
	{
		public static WebApplicationBuilder RegisterAuthentication(this WebApplicationBuilder builder)
		{
			var jwtSettings = new JwtSettings();
			builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);

			var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
			builder.Services.Configure<JwtSettings>(jwtSection);

			builder.Services
				.AddAuthentication(a =>
				{
					a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
					a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(jwt =>
				{
					jwt.SaveToken = true;
					jwt.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
							.GetBytes(jwtSettings.SigningKey ?? throw new InvalidOperationException())),
						ValidateIssuer = true,
						ValidIssuer = jwtSettings.Issuer,
						ValidateAudience = true,
						ValidAudiences = jwtSettings.Audiences,
						RequireExpirationTime = false,
						ValidateLifetime = true
					};
					jwt.Audience = jwtSettings.Audiences?[0];
					jwt.ClaimsIssuer = jwtSettings.Issuer;
				});

			builder.Services.Configure<IdentityOptions>(options =>
			{
				options.SignIn.RequireConfirmedEmail = true;
			});

			builder.Services.AddIdentityCore<IdentityUser>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 5;
				options.Password.RequireNonAlphanumeric = false;
			})
			.AddDefaultTokenProviders()
			.AddRoles<IdentityRole>()
			.AddSignInManager()
			.AddEntityFrameworkStores<DataContext>();

			return builder;
		}

		public static IServiceCollection AddSwagger(this IServiceCollection services)
		{
			services.AddSwaggerGen(option =>
			{
				option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
				option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
				{
					In = ParameterLocation.Header,
					Description = "Please enter a valid token",
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					BearerFormat = "JWT",
					Scheme = "Bearer"
				});

				option.AddSecurityRequirement(new OpenApiSecurityRequirement()
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer",
							}
						},
						new string[]{}
					}
				});
			});

			return services;
		}
	}
}

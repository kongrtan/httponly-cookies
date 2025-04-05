using System.IdentityModel.Tokens.Jwt;
using BlazorWasmCookieAuth.Api;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();



JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
		options.Authority = "https://demo.identityserver.io/";
		options.Audience = "api";

		options.TokenValidationParameters = new TokenValidationParameters {
			NameClaimType = JwtClaimTypes.Name,
			RoleClaimType = JwtClaimTypes.Role,
		};
	});

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazorWasmCookieAuth", Version = "v1" });

	c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
		Type = SecuritySchemeType.OAuth2,
		Flows = new OpenApiOAuthFlows {
			AuthorizationCode = new OpenApiOAuthFlow {
				AuthorizationUrl = new Uri("https://demo.identityserver.io/connect/authorize"),
				TokenUrl = new Uri("https://demo.identityserver.io/connect/token"),
				Scopes = {
								{ "openid", "openid" },
								{ "profile", "profile" },
								{ "email", "email" },
								{ "api", "api" },
								{ "offline_access", "offline_access" },
							},
			},
		},
	});

	c.OperationFilter<SwaggerSecurityRequirementsOperationFilter>();
});








var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorWasmCookieAuth");

	c.OAuthAppName("BlazorWasmCookieAuth API");
	c.OAuthClientId("interactive.public");
	c.OAuthScopes("openid", "profile", "email", "api", "offline_access");
	c.OAuthUsePkce();
});

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
});

app.Run();

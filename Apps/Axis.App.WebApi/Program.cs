using Axis.Data.DatabaseConnection;
using Axis.Identity.Abstraction;
using Axis.Web.Extension.Handlers;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add configurations
builder.Configuration.SetBasePath(Path.Combine(builder.Environment.ContentRootPath, "contents", "settings"));
builder.Configuration.AddJsonFile("environment.json", false, false);
builder.Configuration.AddJsonFile("encryption.json", false, false);
builder.Configuration.AddDbConnections(
  options => {
    options.Path = Path.Combine(
      builder.Environment.ContentRootPath,
      builder.Configuration["Paths:DatabaseFile"] ?? "contents/settings/db"
    );
  });

// Add serilog log service
builder.Host.UseSerilog((context, provider, config)
  => config.ReadFrom.Configuration(context.Configuration));
// map serilog to ms logger
builder.Services.AddSingleton(new SerilogLoggerFactory().CreateLogger("app"));
builder.Services.AddLogging();

// Add authentication
builder.Services
  .AddAuthentication(
    options => {
      options.DefaultScheme = "Bearer";
      options.DefaultChallengeScheme = "Bearer";
    })
  .AddJwtBearer(
    options => {
      options.IncludeErrorDetails = true;
      options.TokenValidationParameters = new TokenValidationParameters {
        NameClaimType = "username",
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
          System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
      };
    });

// Add authorization
builder.Services.AddAuthorization(options
  => {
    AuthorizationPolicyBuilder policy = new("Bearer");
    policy.RequireAuthenticatedUser();
    policy.RequireClaim("jti");
    policy.AddRequirements(new TokenAuthorizationRequiremnt());
    options.DefaultPolicy = policy.Build();
  });
// Block the logged out token
builder.Services.AddScoped<IAuthorizationHandler, TokenAuthorizationHandler>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Cors
builder.Services.AddCors(
  options => {
    options.AddPolicy("default",
      policy => {
        policy
          .WithOrigins("*")
          .AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
      });
  });

// Add Hangfire
builder.Services
  .AddHangfire(
    options => {
      options.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("default"));
    })
  .AddHangfireServer(
    options => {
      options.WorkerCount = Environment.ProcessorCount;
      options.Queues = new[] { builder.Environment.EnvironmentName };
    });

// Add context accessor
builder.Services.AddHttpContextAccessor();

// Add signalr
builder.Services.AddSignalR();

// Add exception filter
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add mediatr
builder.Services.AddMediatR(System.Reflection.Assembly.GetExecutingAssembly());

// Add healthy check
builder.Services.AddHealthChecks();

// Add spa static files
builder.Services.AddSpaStaticFiles(
  options => {
    options.RootPath = "ClientApp/dist";
  });

// Add controller
builder.Services
  .AddControllers(
    options => {
      options.Filters.Add<ActionResultHandler>();
    })
  .AddPlugins();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

// exception
app.UseExceptionResultHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

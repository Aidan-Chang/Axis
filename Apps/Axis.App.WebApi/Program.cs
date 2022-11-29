using Axis.Data.Database.Configuration;
using Axis.Data.Database.NamingConvention;
using Axis.Data.SqlBuilder.Execution;
using Axis.Identity.Authencation.Jwt;
using Axis.Message.RabbitMq;
using Axis.Message.SignalR.Hubs;
using Axis.Plugin.AspNetCore;
using Axis.Web.Extension.Common.Handlers;
using Axis.Web.Extension.Worker.Filters;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
      builder.Configuration["Paths:Database"] ?? "contents/databases"
    );
  });

// Add serilog log service
builder.Host.UseSerilog((context, provider, config)
  => config.ReadFrom.Configuration(context.Configuration));
// map serilog to ms logger
builder.Services.AddSingleton(new SerilogLoggerFactory().CreateLogger("api"));
builder.Services.AddLogging();

// add plugin loader
builder.Host.UsePluginLoader(options =>
  builder.Configuration.GetSection("Plugins").Bind(options));

// Add authentication
builder.Services
  .AddAuthentication(options => {
    options.DefaultScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
  })
  .AddJwtBearer(options => {
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
builder.Services.AddAuthorization(options => {
  AuthorizationPolicyBuilder policy = new("Jwt");
  policy.RequireAuthenticatedUser();
  policy.RequireClaim("jti");
  policy.AddRequirements(new TokenAuthorizationRequiremnt());
  options.DefaultPolicy = policy.Build();
});
// Block the logged out token
builder.Services.AddScoped<IAuthorizationHandler, TokenAuthorizationHandler>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
  options.AddSecurityDefinition("Bearer", new() {
    Name = "Authorization",
    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
    Type = SecuritySchemeType.ApiKey,
    In = ParameterLocation.Header,
    Scheme = "Bearer",
  });
  options.AddSecurityRequirement(new() { {
      new OpenApiSecurityScheme {
        Name = "Bearer",
        In = ParameterLocation.Header,
        Reference = new() {
          Id = "Bearer",
          Type = ReferenceType.SecurityScheme,
        }
      },
      new List<string>() }
    });
  options.SwaggerDoc(
    "v1",
    builder.Configuration.GetSection("Swagger:v1").Get<OpenApiInfo>()
    );
});

// Add Cors
builder.Services.AddCors(options => {
  options.AddPolicy("default", policy => {
    policy
      .WithOrigins("*")
      .AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
  });
});

// Add query factory
builder.Services.AddScoped(provider => new QueryFactory());
builder.Services.AddSingleton<Func<QueryFactory>>(
  provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<QueryFactory>());

// Add database context builder
builder.Services.AddSingleton(provider =>
  new Action<DbContextOptionsBuilder>(options => {
    options
      .EnableSensitiveDataLogging()
      .UseLoggerFactory(provider.GetService<ILoggerFactory>())
      .UseNpgsql(builder.Configuration.GetConnectionString("default"), x => x.MigrationsHistoryTable("migrations", "app"))
      .UseNamingConvention(name: builder.Configuration["NamingConvention"] ?? "CamelCase");
  }));

// Add Hangfire
builder.Services
  .AddHangfire(options => {
    options.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("default"));
  })
  .AddHangfireServer(options => {
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
  .AddControllers(options => {
    options.Filters.Add<ActionResultHandler>();
  })
  .AddPlugins();

var app = builder.Build();

// Configure the HTTP request pipeline.

// apply swagger
app.UseSwagger(options => {
  options.RouteTemplate = $"{app.Configuration["Paths:Service"]}/api/doc/{{documentName}}.{{json|yaml}}";
});
app.UseSwaggerUI(options => {
  options.RoutePrefix = $"{app.Configuration["Paths:Service"]}/api";
  options.SwaggerEndpoint($"doc/v1.json", "v1");
});

// apply log request to serilog
app.UseSerilogRequestLogging();

// apply exception
if (app.Environment.IsDevelopment() || app.Environment.IsStaging()) {
  app.UseDeveloperExceptionPage();
  //app.UseElmahExceptionPage();
  app.UseMigrationsEndPoint();
}
app.UseExceptionResultHandler();

// apply to force https redirection
app.UseHttpsRedirection();
//app.UseHsts();

// apply cors
app.UseCors("default");

// apply static files
app.UseDefaultFiles();
app.UseStaticFiles();

// apply rabbit message queue
app.UseRabbitMq(options => { });

// apply authorization
app.UseAuthentication();
app.UseAuthorization();

// apply plugins
app.UsePlugins();

// endpoints
app.MapControllers();
app.MapHub<MessageHub>($"/{app.Configuration["Paths:Service"] ?? "services"}/message");
app.MapHub<ProgressHub>($"/{app.Configuration["Paths:Service"] ?? "services"}/progress");
app.MapHealthChecks($"/{app.Configuration["Paths:Service"]}/healthz");
app.MapHangfireDashboard($"/{app.Configuration["Paths:Service"] ?? "services"}/worker",
  new DashboardOptions {
    AppPath = null,
    DisplayStorageConnectionString = false,
    DashboardTitle = "Task Dashboard",
    Authorization = new[] { new HangfireAuthorizeFilter() },
    IsReadOnlyFunc = (content) => content.IsReadOnly("admins"),
  });

// spa
app.UseSpa(options => {
  options.Options.SourcePath = "ClientApp";
  if (app.Environment.IsDevelopment()) {
    options.UseProxyToSpaDevelopmentServer("http://localhost:4200");
  }
});

app.Run();

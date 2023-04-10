using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using snapnow.DAOS;
using snapnow.DAOS.MssqlDbImplementation;
using snapnow.Data;
using snapnow.Models;
using snapnow.Services;


var builder = WebApplication.CreateBuilder(args);
// Add logger
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var myAllowSpecificOrigins = "MyAllowSpecificOrigins";
builder.Services.AddCors(cors => {
    cors.AddPolicy(name: myAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://127.0.0.1:8080", "http://127.0.0.1:5500", "127.0.0.1:5500", "127.0.0.1:8080").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRoleDao, RoleDaoMssqlDatabase>();
builder.Services.AddScoped<IUserDao, UserDaoMssqlDatabase>();
builder.Services.AddScoped<IUserService, UserServiceJwtCookie>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddTransient<IMailService, MailService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// Add identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 5;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(auth =>
    {
        auth.RequireAuthenticatedSignIn = false;
        auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        auth.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AuthSettings:Audience"],
            ValidIssuer = builder.Configuration["AuthSettings:Issuer"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthSettings:Key"]))

        };
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["Token"];
                return Task.CompletedTask;
            }
        };
    }).AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["GoogleAuthSettingsMobile:ClientId"];
        options.ClientSecret = builder.Configuration["GoogleAuthSettingsMobile:ClientSecret"];
        options.CallbackPath = "/api/authentication/signin-google";
    });

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Jwt auth header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://127.0.0.1:8080",
        "http://127.0.0.1:5500", "127.0.0.1:5500", "127.0.0.1:8080");
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
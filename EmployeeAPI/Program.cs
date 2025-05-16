using System.Text;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Services.CheckinServices;
using EmployeeAPI.Services.DepartmentServices;
using EmployeeAPI.Services.DutyServices;
using EmployeeAPI.Services.FileServices;
using EmployeeAPI.Services.PositionServices;
using EmployeeAPI.Services.StaffServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSetting = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.AddScoped<IStaffRepository, EFStaffRepository>();
builder.Services.AddScoped<IDutyRepository, EFDutyRepository>();
builder.Services.AddScoped<IDepartmentRepository, EFDepartmentRepository>();
builder.Services.AddScoped<IPositionRepository, EFPositionRepository>();
builder.Services.AddScoped<IAuthRepository, EFAuthRepository>();
builder.Services.AddScoped<ICheckinRepository, EFCheckinRepository>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDutyService, DutyService>();
builder.Services.AddScoped<IStaffService, StafffService>();
builder.Services.AddScoped<ICheckinService, CheckinService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {your token}"
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
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSetting.Issuer,
            ValidAudience = jwtSetting.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSetting.Key))
        };
    });
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

app.UseSwagger();           
app.UseSwaggerUI();      
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

using System.Reflection;
using System.Text;
using EmployeeAPI.Attributes;
using EmployeeAPI.Middlewares;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.LogStatusConfigs;
using EmployeeAPI.Repositories.Departments;
//using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Repositories.Holidays;
using EmployeeAPI.Repositories.Payrolls;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Repositories.ScheduleTimes;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.AllowedIpServices;
using EmployeeAPI.Services.AuthServices;
using EmployeeAPI.Services.CheckinServices;
using EmployeeAPI.Services.LogStatusConfigServices;
using EmployeeAPI.Services.DepartmentServices;
using EmployeeAPI.Services.DutyServices;
using EmployeeAPI.Services.HolidayServices;
using EmployeeAPI.Services.ImageServices;
using EmployeeAPI.Services.PayrollServices;
using EmployeeAPI.Services.PositionServices;
using EmployeeAPI.Services.ScheduleTimeServices;
using EmployeeAPI.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EmployeeAPI.Services.Dashboards;
using EmployeeAPI.Repositories.Companies;
using EmployeeAPI.Services.CompanyServices;

var builder = WebApplication.CreateBuilder(args);
var CustomCors = "_customCors";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CustomCors",
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                                 //.AllowCredentials();
                      });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EmployeeDb")));


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSetting = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<GoogleSheetSettings>(builder.Configuration.GetSection("GoogleSheets"));
builder.Services.AddScoped<GoogleSheetHelper>();

//builder.Services.AddHostedService<AbsentBackgroundService>();//////////////////////////background serrvice

//builder.Services.AddScoped<IDutyRepository, EFDutyRepository>();
builder.Services.AddScoped<IDepartmentRepository, EFDepartmentRepository>();
builder.Services.AddScoped<IPositionRepository, EFPositionRepository>();
builder.Services.AddScoped<IAuthRepository, EFAuthRepository>();
builder.Services.AddScoped<ICheckinRepository, EFCheckinRepository>();
builder.Services.AddScoped<IPayrollRepository, EFPayrollRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<IScheduleTimeRepository, EFScheduleTimeRepository>();
builder.Services.AddScoped<ILogStatusConfigRepository, EFLogStatusConfigRepository>();
builder.Services.AddScoped<IAllowedIPRepository, EFAllowedIPRepository>();
builder.Services.AddScoped<IHolidayRepository, EFHolidayRepository>();
builder.Services.AddScoped<ICompanyRepository, EFCompanyRepository>();

builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDutyService, DutyService>();
builder.Services.AddScoped<ICheckinService, CheckinService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IAuthService,  AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IScheduleTimeService, ScheduleTimeService>();
builder.Services.AddScoped<ILogStatusConfigService, LogStatusConfigService>();
builder.Services.AddScoped<IAllowedIPService, AllowedIPService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<ICloudImageService, CloudImageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Employee API", Version = "v1" });
    c.DocumentFilter<SwaggerControllerOrderAttribute>();
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Key))
        };
    });

builder.Services.AddControllers();
//Ý nghĩa: Đăng ký dịch vụ Controllers vào Dependency Injection (DI container).
//Tác dụng: Cho phép bạn sử dụng các Controller (thường nằm trong thư mục Controllers/) để xử lý các HTTP request (GET, POST, PUT, DELETE...).
builder.Services.AddAuthorization();
//Ý nghĩa: Đăng ký middleware Authorization.
//Tác dụng: Cho phép kiểm tra quyền truy cập người dùng đối với từng endpoint (ví dụ: [Authorize] trên controller hoặc action).
builder.Logging.ClearProviders();
//Ý nghĩa: Xóa tất cả các "provider" ghi log mặc định.
//Tác dụng: Giúp bạn tùy chỉnh lại hệ thống logging (ghi log), thường dùng khi bạn không muốn log vào những nơi mặc định như debug output hoặc Event Log.
builder.Logging.AddConsole();
//Ý nghĩa: Thêm một provider để ghi log ra Console.
//Tác dụng: Hiển thị log trong Terminal, Command Prompt hoặc Output window của Visual Studio.
var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    SeedDB.SeedAdminUser(dbContext);
//    await SeedDB.SeedHolidayAsync(dbContext);
//}

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});
/*app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = errorFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal Server Error");
    });
});*/
app.UseSwagger();           
app.UseSwaggerUI();      
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("CustomCors");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TokenVersionMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();

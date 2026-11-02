using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WadiAlAin.Api.Middleware;
using WadiAlAin.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة خدمات البنية التحتية (Infrastructure DI)
builder.Services.AddInfrastructure(builder.Configuration);

// 2. إعداد المصادقة عبر JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "WadiAlAin_Quality_System_Super_Secret_Jwt_Key_2026_Secure_Token!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "WadiAlAin.Api",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "WadiAlAin.App",
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. إعداد CORS للسماح بالاتصال من تطبيقات Flutter والويب
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 4. إعداد Swagger للتوثيق مع زر Authorize الذي يضيف Bearer تلقائياً
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Wadi Al-Ain Factory Quality API",
        Version = "v1",
        Description = "نظام إدارة الجودة وسلامة الغذاء - مياه وادي العين (Clean Architecture)"
    });

    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "الصق قيمة الـ JWT token هنا مباشرة (بدون كتابة كلمة Bearer)."
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wadi Al-Ain Quality API v1");
    c.RoutePrefix = string.Empty; // فتح Swagger مباشرة في الصفحة الرئيسية
});

app.UseCors("AllowAll");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws/notifications")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var wsHub = context.RequestServices.GetRequiredService<WadiAlAin.Infrastructure.Services.RealtimeNotificationHub>();
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await wsHub.HandleWebSocketAsync(context, webSocket);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

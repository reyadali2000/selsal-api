using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Infrastructure.Authentication;
using WadiAlAin.Infrastructure.Persistence;
using WadiAlAin.Infrastructure.Services;

namespace WadiAlAin.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
            services.AddSingleton<JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IWaterQualityService, WaterQualityService>();
            services.AddScoped<IHygieneService, HygieneService>();
            services.AddScoped<IRawMaterialsService, RawMaterialsService>();
            services.AddScoped<IDynamicReportService, DynamicReportService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<INotificationsService, NotificationsService>();
            services.AddSingleton<RealtimeNotificationHub>();
            services.AddSingleton<IRealtimeNotificationHub>(sp => sp.GetRequiredService<RealtimeNotificationHub>());

            return services;
        }
    }
}
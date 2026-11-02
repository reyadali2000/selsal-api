# 1. مرحلة البناء (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# نسخ ملفات المشاريع والـ Solution
COPY WadiAlAin.sln .
COPY src/WadiAlAin.Domain/WadiAlAin.Domain.csproj src/WadiAlAin.Domain/
COPY src/WadiAlAin.Application/WadiAlAin.Application.csproj src/WadiAlAin.Application/
COPY src/WadiAlAin.Infrastructure/WadiAlAin.Infrastructure.csproj src/WadiAlAin.Infrastructure/
COPY src/WadiAlAin.Api/WadiAlAin.Api.csproj src/WadiAlAin.Api/

RUN dotnet restore

# نسخ بقية الكود وبناء النسخة النهائية
COPY . .
WORKDIR /app/src/WadiAlAin.Api
RUN dotnet publish -c Release -o /out

# 2. مرحلة التشغيل (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "WadiAlAin.Api.dll"]
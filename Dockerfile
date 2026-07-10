FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо solution та всі csproj файли для відновлення залежностей
COPY ["PDR_Project.sln", "./"]
COPY ["TrafficRules.Api/TrafficRules.Api.csproj", "TrafficRules.Api/"]
COPY ["TrafficRules.Application/TrafficRules.Application.csproj", "TrafficRules.Application/"]
COPY ["TrafficRules.Domain/TrafficRules.Domain.csproj", "TrafficRules.Domain/"]
COPY ["TrafficRules.Infrastructure/TrafficRules.Infrastructure.csproj", "TrafficRules.Infrastructure/"]

RUN dotnet restore "TrafficRules.Api/TrafficRules.Api.csproj"

# Копіюємо весь інший код
COPY . .
WORKDIR "/src/TrafficRules.Api"
RUN dotnet build "TrafficRules.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrafficRules.Api.csproj" -c Release -o /app/publish

# Фінальний образ для запуску (легкий)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Вказуємо, що сервіс має слухати на порту 8080 (стандарт для .NET 8 у контейнерах)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Створюємо директорію для зображень
RUN mkdir -p /app/wwwroot/images

# Запускаємо API
ENTRYPOINT ["dotnet", "TrafficRules.Api.dll"]

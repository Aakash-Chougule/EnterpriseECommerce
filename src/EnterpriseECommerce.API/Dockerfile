FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY EnterpriseECommerce.slnx ./

COPY src/EnterpriseECommerce.Domain/EnterpriseECommerce.Domain.csproj \
     src/EnterpriseECommerce.Domain/

COPY src/EnterpriseECommerce.Application/EnterpriseECommerce.Application.csproj \
     src/EnterpriseECommerce.Application/

COPY src/EnterpriseECommerce.Infrastructure/EnterpriseECommerce.Infrastructure.csproj \
     src/EnterpriseECommerce.Infrastructure/

COPY src/EnterpriseECommerce.API/EnterpriseECommerce.API.csproj \
     src/EnterpriseECommerce.API/

RUN dotnet restore \
    src/EnterpriseECommerce.API/EnterpriseECommerce.API.csproj

COPY src ./src

RUN dotnet publish \
    src/EnterpriseECommerce.API/EnterpriseECommerce.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EnterpriseECommerce.API.dll"]
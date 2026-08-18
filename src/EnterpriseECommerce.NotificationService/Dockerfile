FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src/EnterpriseECommerce.NotificationService/EnterpriseECommerce.NotificationService.csproj \
     src/EnterpriseECommerce.NotificationService/

RUN dotnet restore \
    src/EnterpriseECommerce.NotificationService/EnterpriseECommerce.NotificationService.csproj

COPY src/EnterpriseECommerce.NotificationService/ \
     src/EnterpriseECommerce.NotificationService/

RUN rm -rf \
    src/EnterpriseECommerce.NotificationService/obj \
    src/EnterpriseECommerce.NotificationService/bin

RUN dotnet restore \
    src/EnterpriseECommerce.NotificationService/EnterpriseECommerce.NotificationService.csproj

RUN dotnet publish \
    src/EnterpriseECommerce.NotificationService/EnterpriseECommerce.NotificationService.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EnterpriseECommerce.NotificationService.dll"]
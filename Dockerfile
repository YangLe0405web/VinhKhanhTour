FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VinhKhanhTour.Shared/ VinhKhanhTour.Shared/
COPY VinhKhanhTour.Api/ VinhKhanhTour.Api/

RUN dotnet restore VinhKhanhTour.Api/VinhKhanhTour.Api.csproj
RUN dotnet publish VinhKhanhTour.Api/VinhKhanhTour.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENTRYPOINT ["dotnet", "VinhKhanhTour.Api.dll"]
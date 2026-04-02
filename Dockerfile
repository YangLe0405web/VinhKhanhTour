FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VinhKhanhTour.Shared/ VinhKhanhTour.Shared/
COPY VinhKhanhTour.Api/ VinhKhanhTour.Api/

RUN dotnet restore VinhKhanhTour.Api/VinhKhanhTour.Api.csproj
RUN dotnet publish VinhKhanhTour.Api/VinhKhanhTour.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .


ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENTRYPOINT ["sh", "-c", "dotnet VinhKhanhTour.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
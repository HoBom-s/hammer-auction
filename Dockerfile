FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Hammer.Auction.slnx ./
COPY src/Hammer.Auction.Domain/Hammer.Auction.Domain.csproj src/Hammer.Auction.Domain/
COPY src/Hammer.Auction.Application/Hammer.Auction.Application.csproj src/Hammer.Auction.Application/
COPY src/Hammer.Auction.Infrastructure/Hammer.Auction.Infrastructure.csproj src/Hammer.Auction.Infrastructure/
COPY src/Hammer.Auction.Api/Hammer.Auction.Api.csproj src/Hammer.Auction.Api/
RUN dotnet restore src/Hammer.Auction.Api/Hammer.Auction.Api.csproj

COPY src/ src/
RUN dotnet publish src/Hammer.Auction.Api/Hammer.Auction.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup --no-create-home appuser

COPY --from=build /app .

USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Hammer.Auction.Api.dll"]

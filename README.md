# Hammer Auction

Hammer 경매 플랫폼의 Auction Service.

## Stack

- ASP.NET (.NET 10)
- PostgreSQL
- Redis (캐싱)

## Features

- 정규화된 경매 데이터 조회 API
- Redis 기반 캐싱
- Kafka consumer (collector로부터 데이터 수신)

## Services

| Service | Description |
|---------|-------------|
| [hammer-gateway](https://github.com/HoBom-s/hammer-gateway) | API Gateway |
| [hammer-user](https://github.com/HoBom-s/hammer-user) | User & Auth |
| [hammer-auction](https://github.com/HoBom-s/hammer-auction) | Auction API |
| [hammer-collector](https://github.com/HoBom-s/hammer-collector) | Data Collector |
| [hammer-support](https://github.com/HoBom-s/hammer-support) | Logging, FCM, Support |

## Getting Started

```bash
dotnet restore
dotnet run --project src/Hammer.Auction
```

## Branch Strategy

- `main` — Production
- `develop` — Development (default)

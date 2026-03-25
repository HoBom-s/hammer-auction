# Hammer Auction

Hammer 경매 플랫폼의 Auction Service.

## Stack

- ASP.NET (.NET 10)
- PostgreSQL
- Kafka (collector로부터 데이터 수신)

## Features

- KAMCO 공매 데이터 Kafka 소비 및 PostgreSQL 적재
- 경매 물건 목록 조회 API (페이징, 필터링)
- 경매 물건 상세 조회 API
- Scalar OpenAPI 문서 (Development 환경)

## Services

| Service                                                         | Description           |
|-----------------------------------------------------------------|-----------------------|
| [hammer-gateway](https://github.com/HoBom-s/hammer-gateway)     | API Gateway           |
| [hammer-user](https://github.com/HoBom-s/hammer-user)           | User & Auth           |
| [hammer-auction](https://github.com/HoBom-s/hammer-auction)     | Auction API           |
| [hammer-collector](https://github.com/HoBom-s/hammer-collector) | Data Collector        |
| [hammer-support](https://github.com/HoBom-s/hammer-support)     | Logging, FCM, Support |

## Getting Started

### 환경 설정

`.env.example`을 복사해서 `.env.local`을 만들고 값을 채운다.

```bash
cp .env.example .env.local
```

### 로컬 실행

```bash
dotnet run --project src/Hammer.Auction.Api
```

### Docker 실행

```bash
docker build -t hammer-auction .
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=live \
  -e ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=bear;Username=...;Password=...;Search Path=auction" \
  -e Kafka__BootstrapServers="kafka:9092" \
  hammer-auction
```

> `.env` 파일은 로컬 개발용. Docker 배포 시에는 컨테이너 환경변수로 직접 주입한다.

## Branch Strategy

- `main` — Production
- `develop` — Development (default)

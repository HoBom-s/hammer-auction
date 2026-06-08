# Hammer Auction

KAMCO(한국자산관리공사) 온비드 공매 물건 데이터를 저장하고 API로 제공하는 서비스.

## Hammer 플랫폼

Hammer는 온비드 공매 데이터를 수집·가공하여 **공매 투자 진입 장벽을 낮추는 플랫폼**이다.

### 제공 기능

- **물건 탐색** — 카테고리(아파트, 단독주택, 토지, 자동차, 유가증권), 지역, 가격대, 할인율 필터
- **입찰 일정** — 캘린더 뷰로 시작/마감 일정 확인
- **저가 매물 큐레이션** — 할인율 상위 물건, 유찰 물건 모아보기
- **지도 뷰** — 주소 기반 물건 위치 시각화
- **교육 컨텐츠** — 물건 상태/입찰방식별 맥락적 가이드 (물건을 보면서 자연스럽게 배우는 구조)
- **커뮤니티** — 공매 투자자 간 정보 교환, 경험 공유

## Stack

- ASP.NET (.NET 10)
- PostgreSQL + EF Core
- Apache Kafka (consumer)
- Clean Architecture (Domain → Application → Infrastructure → Api)

## Features

- KAMCO 공매 데이터 Kafka 소비 및 PostgreSQL 적재
- 경매 물건 목록 조회 API (페이징, 필터링)
- 경매 물건 상세 조회 API
- Scalar OpenAPI 문서 (Development 환경)

## Services

```
온비드 API → hammer-support(수집) → Kafka → hammer-auction(저장/API) → 프론트엔드
```

## 데이터 처리

### 중복 방지

- Kafka 메시지 key: `{plnmNo}-{pbctNo}-{cltrNo}`
- Application-level UPSERT: 복합키로 조회 후 있으면 UPDATE, 없으면 INSERT
- DB-level unique index: `ix_kamco_auction_items_plnm_no_pbct_no_cltr_no`

### 상태 관리

배치가 돌 때마다 `pbctCltrStatNm`이 최신 상태로 갱신된다. 별도 상태 이력 테이블 없이 현재 상태 기반 필터링으로 운영.

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

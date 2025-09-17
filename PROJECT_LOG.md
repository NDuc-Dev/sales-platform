
# Sales Platform Project Log
**Angular 18 + .NET 8 Microservices**  
**Editor:** VS Code · **OS:** (dev local) · **Generated:** 2025-09-18 05:49:42 UTC+07:00

---

## 0) Thông tin dự án
- **Tên dự án:** Sales Platform (Storefront + Microservices)
- **Mục tiêu:** Trang web quản lý bán hàng (thời trang/giày dép) với FE Angular 18 & BE .NET 8, kiến trúc microservices, có Gateway, Auth (JWT), Catalog, Inventory (sau), Basket (sau), Order (sau)…
- **Kiểu repo:** monorepo
- **Ngôn ngữ:** TypeScript (FE), C# (.NET, BE)
- **IDE:** Visual Studio Code
- **Quy ước commit:** Conventional Commits (`feat:`, `fix:`, `chore:`…)

---

## 1) Kiến trúc & công nghệ
### Front-end
- Angular **18** (standalone, Vite builder), TypeScript, RxJS, Angular Signals
- UI: TailwindCSS + Angular Material (tùy dùng)
- Routing: Standalone Router, lazy loading
- HTTP: `provideHttpClient(withFetch(), withInterceptors([...]))`
- Testing: Jest (unit), Playwright (e2e) – (chưa bật)
- **OpenAPI client**: `ng-openapi-gen` (chỉ sinh **fn** + **models**, _không_ sinh services/module)

### Back-end (Microservices – chạy dev local)
- ASP.NET Core WEB API (.NET **8**)
- **Gateway:** YARP Reverse Proxy
- **Identity.Api:** ASP.NET Core Identity + JWT
- **Catalog.Api:** sản phẩm + DTO + EF Core
- CSDL: PostgreSQL 16 (mỗi service 1 DB)
- Message broker: RabbitMQ (dự kiến dùng khi có sự kiện)
- Cache: Redis (Basket sau này)
- ORM: EF Core 8 + **Migrations** (không dùng `EnsureCreated`)
- Observability: Swagger/Swashbuckle, Health endpoints

### Hạ tầng Dev (Docker Compose)
- Postgres (5432), pgAdmin (5050), Redis (6379), Redis-Commander (8081), RabbitMQ (5672/15672)

---

## 2) Cấu trúc thư mục
```
sales-platform/
  backend/
    Gateway/
    Identity.Api/
    Catalog.Api/
  frontend/
    storefront/
      openapi/
        identity.json
        catalog.json
      src/app/core/
        api-clients/     # sinh tự động bởi ng-openapi-gen (fn + models)
        auth/
        api/             # facades do dev viết
      src/pages/
        shop/
        admin/
        auth/
  infrastructure/
    docker-compose.yml
  env/
```

---

## 3) Cổng & endpoint (dev)
| Thành phần   | URL/Port                          | Ghi chú |
|--------------|-----------------------------------|--------|
| Gateway      | http://localhost:7000             | YARP; CORS allow `http://localhost:4200` |
| Identity.Api | http://localhost:7001             | Swagger + `/api/auth/*` + `/health` |
| Catalog.Api  | http://localhost:7002             | Swagger + `/api/catalog/*` + `/health` |
| Frontend     | http://localhost:4200             | `pnpm ng serve` |

---

## 4) Nhật ký thực hiện

### Ngày 1 — Scaffold & chạy end-to-end (MVP khung)
**Hoàn thành:**
- Tạo monorepo & cấu trúc thư mục
- Docker Compose: Postgres, Redis, RabbitMQ, pgAdmin, Redis-Commander
- **Gateway** (YARP) map route:
  - `/api/auth/{**catch-all}` → `http://localhost:7001`
  - `/api/catalog/{**catch-all}` → `http://localhost:7002`
- **Identity.Api**: stub endpoints `register/login` + `/health`
- **Catalog.Api**: DB + seeding mẫu (Products), `/api/catalog/products`, `/health`
- **Frontend**: Angular 18 + Tailwind; trang `/shop` gọi API qua Gateway

**Lệnh thường dùng:**
```bash
# hạ tầng
docker compose -f infrastructure/docker-compose.yml up -d

# chạy Gateway/Identity/Catalog (bind 0.0.0.0 để SSR/Node nhìn thấy)
dotnet run --project backend/Gateway/Gateway.csproj --urls http://0.0.0.0:7000
dotnet run --project backend/Identity.Api/Identity.Api.csproj --urls http://0.0.0.0:7001
dotnet run --project backend/Catalog.Api/Catalog.Api.csproj --urls http://0.0.0.0:7002

# FE
cd frontend/storefront
pnpm ng serve
```

**Sự cố & cách xử (Day 1):**
- `NullInjectorError: No provider for HttpClient` → Thêm `provideHttpClient()` vào `app.config.ts`
- CORS → Thêm CORS policy ở Gateway cho `http://localhost:4200`

---

### Ngày 2 — Auth chuẩn + DTO + OpenAPI + FE login
**Hoàn thành:**
- **Identity.Api**
  - Thêm `AppUser : IdentityUser` + `AppDbContext : IdentityDbContext<AppUser>`
  - `JwtBearer` + endpoints: `/api/auth/register`, `/api/auth/login`, `/api/auth/seed-admin` (DEV-only)
  - EF **Migrations**: `dotnet ef migrations add InitIdentity && dotnet ef database update`
- **Catalog.Api**
  - Chuyển endpoint **KHÔNG** dùng anonymous type → dùng DTO:
    - `ProductListItemDto`, `ProductDetailDto`
    - `.Produces<T>()` + `.WithTags("Products")`
  - Bật `JwtBearer` + endpoint test admin: `/api/catalog/admin/ping` (RequireRole("Admin"))
  - EF **Migrations**: `InitCatalog` (+ xử lỗi 42P07 bằng drop DB hoặc drop schema)
- **Gateway**
  - YARP routes hoạt động; `/health` OK
- **OpenAPI (Angular)**
  - Cài `ng-openapi-gen`
  - **Config tách 2 file**: `openapi/identity.json`, `openapi/catalog.json`
  - **Chỉ sinh fn + models**: `"services": false, "module": false`
  - Scripts:
    ```json
    "clean:api": "rimraf src/app/core/api-clients",
    "openapi:identity": "ng-openapi-gen -c openapi/identity.json",
    "openapi:catalog": "ng-openapi-gen -c openapi/catalog.json",
    "openapi:regen": "pnpm run clean:api && pnpm run openapi:identity && pnpm run openapi:catalog"
    ```
  - Generate: `pnpm run openapi:regen`
- **Frontend**
  - `app.config.ts`: `provideHttpClient(withFetch(), withInterceptors([authInterceptor]))`
  - `AuthService`: dùng fn `apiAuthLoginPost` / `apiAuthRegisterPost`; SSR-safe (`isPlatformBrowser` cho `localStorage`)
  - `CatalogFacade`: gọi `apiCatalogProductsGet(...).pipe(map(r => r.body))`
  - `HomePage`: hiển thị list sản phẩm
  - `LoginPage`: đăng nhập → `setSession()` → điều hướng `/admin`
  - `admin.guard.ts`: chặn nếu không có token/role Admin

**Smoke test:**
```bash
# Gateway/Services
curl http://localhost:7000/health
curl http://localhost:7002/api/catalog/products       # trực tiếp service
curl http://localhost:7000/api/catalog/products       # qua Gateway

# FE
# - /shop hiển thị list sản phẩm
# - /login đăng nhập admin@local / Admin123$
# - /admin vào được
```

**Troubleshooting đáng nhớ (Day 2):**
- **Top-level statements** vs `namespace` trong Program.cs → tách class/record ra file riêng hoặc bọc Main “classic”
- **OpenAPI lỗi anonymous**: `Int32StringStringInt32<>f__AnonymousType0` → Dùng DTO có tên + `.Produces<T>()`
- **SSR warning `NG02801`** → thêm `withFetch()`
- **`localStorage is not defined`** (SSR) → dùng `isPlatformBrowser(...)`
- **HTTP 0 / `fetch failed`** → ECONNREFUSED: kiểm tra service/port/binding `--urls http://0.0.0.0:<port>`; thử gọi thẳng 7002 để cô lập vấn đề gateway

---

## 5) Quy ước & lưu ý vận hành
- **Không dùng `EnsureCreated()`** sau khi đã chuyển sang Migrations.
- **Regenerate OpenAPI** mỗi khi đổi contract BE: `pnpm run openapi:regen`
- **Chỉ gọi `/api/auth/seed-admin`** trong DEV, _không_ expose cho FE/production.
- FE SSR-safe: mọi truy cập Web APIs (localStorage, window, document) cần guard `isPlatformBrowser`.
- Gateway forward `Authorization` header mặc định (OK cho JWT).

---

## 6) Kế hoạch tiếp theo (Ngày 3 — đề xuất)
- Auth nâng cao: **Refresh token**, revoke/rotate, token storage an toàn
- Catalog Admin CRUD (create/update/delete product & variants) + upload ảnh
- Hoàn thiện **Interceptor**: xử lý 401 tự refresh token (nếu có)
- Setup **Jest** (FE) + **xUnit** (BE) test cơ bản
- Tài liệu hoá `README` + scripts “1-click dev” (Makefile/PS1)

---

## 7) Phụ lục: Lệnh nhanh hữu ích
```bash
# EF Tools
dotnet tool install -g dotnet-ef
dotnet ef migrations add <Name> --project backend/ServiceName
dotnet ef database update --project backend/ServiceName
dotnet ef database drop --project backend/ServiceName --force

# Docker infra
docker compose -f infrastructure/docker-compose.yml up -d
docker compose -f infrastructure/docker-compose.yml down

# OpenAPI generate
pnpm run openapi:regen
```

---

## 8) Change Log
- 2025-09-18 05:49:42 UTC+07:00 — Tạo file log & ghi lại Ngày 1–2, cấu hình hiện tại, quy ước & kế hoạch Ngày 3.

---

> ✅ **Gợi ý cách dùng:** Sau mỗi ngày làm việc, hãy gửi cho ChatGPT “Cập nhật log Ngày N: …” kèm thay đổi/commit chính, mình sẽ bổ sung vào file này và trả về bản mới.

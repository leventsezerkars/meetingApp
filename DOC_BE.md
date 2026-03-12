# Toplantı Uygulaması — Backend Teknik Dokümanı

Bu doküman, **ToplantiApp** backend tarafında kullanılan teknolojileri, mimari yapıyı (Clean Architecture / DDD), proje katmanlarını ve teknik detayları açıklar.

---

## 1. Kullanılan Teknolojiler

| Teknoloji | Sürüm | Amaç |
|-----------|--------|------|
| **.NET** | 9.0 | Çalışma zamanı ve SDK |
| **ASP.NET Core** | 9.x | Web API, middleware, Kestrel |
| **Entity Framework Core** | 9.0.3 | ORM, SQL Server, migration, interceptor |
| **SQL Server** | — | Veritabanı |
| **MediatR** | 14.1 | CQRS: Command/Query ve handler’lar |
| **FluentValidation** | 12.1 | Request validasyonu |
| **AutoMapper** | 12.x | Entity ↔ DTO mapping |
| **JWT Bearer** | 9.0.3 | Kimlik doğrulama |
| **BCrypt.Net-Next** | 4.1 | Şifre hashleme |
| **MailKit** | 4.15 | SMTP ile e-posta (davet, bildirim) |
| **Quartz.NET** | 3.16 | Zamanlanmış işler (iptal edilmiş toplantı temizliği) |
| **Swashbuckle (Swagger)** | 6.9 | OpenAPI dokümantasyonu ve UI |

---

## 2. Mimari: Clean Architecture / DDD

Çözüm dört ana katmandan oluşur; bağımlılıklar içe doğru (Domain’e) akar.

```
┌─────────────────────────────────────────────────────────┐
│  ToplantiApp.API          (Sunum / HTTP)                 │
│  Controllers, Middleware, CurrentUserProvider            │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│  ToplantiApp.Application  (İş kuralları, CQRS)          │
│  Commands, Queries, Handlers, DTOs, Validators, Mapping  │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│  ToplantiApp.Infrastructure (Veri, dış servisler)        │
│  DbContext, Repositories, EF Config, Mail, File, Jobs   │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│  ToplantiApp.Domain       (Çekirdek)                     │
│  Entities, Enums, Repository/Service arayüzleri          │
└─────────────────────────────────────────────────────────┘
```

- **API**: Domain’e doğrudan referans vermez; Application ve Infrastructure kullanır.
- **Application**: Domain’e referans verir; Infrastructure’a referans vermez.
- **Infrastructure**: Domain ve Application’a referans verir; uygulama detaylarını (EF, Mail, File) içerir.
- **Domain**: Hiçbir projeye bağımlı değildir; sadece entity’ler, enum’lar ve interface’ler.

---

## 3. Katmanlar ve Proje İçeriği

### 3.1 ToplantiApp.Domain

- **Amaç**: İş mantığının çekirdeği; framework’ten bağımsız.
- **İçerik**:
  - **Entities**: `User`, `Meeting`, `MeetingParticipant`, `MeetingDocument`, `MeetingLog`; `BaseEntity` (Id, CreatedAt, CreatedByUserId), `AuditableEntity` (UpdatedAt, UpdatedByUserId).
  - **Enums**: `MeetingStatus` (Active, Cancelled), `ParticipantType` (Internal, External).
  - **Interfaces**: `IGenericRepository<T>`, `IMeetingRepository`, `IUserRepository`, `IMeetingParticipantRepository`, `IUnitOfWork`, `ITokenService`, `IMailService`, `IFileService`.
- **Bağımlılık**: Ek paket yok; sadece .NET SDK.

### 3.2 ToplantiApp.Application

- **Amaç**: Use case’ler (CQRS), DTO’lar, validasyon, mapping.
- **İçerik**:
  - **DTOs**: CreateMeetingDto, UpdateMeetingDto, MeetingDto, MeetingListDto, MeetingRoomDto, ParticipantDto, MeetingDocumentDto, MeetingAccessResult, Auth ile ilgili DTO’lar.
  - **Common**: `ICurrentUserProvider`, `Response` / `Response<T>`, `PaginatedResponse<T>`, `PaginationRequest` (Models); `PaginationExtensions` (Extensions); `ValidationBehavior` (MediatR pipeline); `AppException`, `NotFoundException`, `ForbiddenException`, `ConflictException` (Exceptions).
  - **Features**:
    - **Auth**: LoginCommand, RegisterCommand (FluentValidation ile).
    - **Meetings**: CreateMeetingCommand, UpdateMeetingCommand, DeleteMeetingCommand, CancelMeetingCommand; GetMeetingsQuery (sayfalı), GetMeetingByIdQuery; UploadMeetingDocumentCommand.
    - **Participants**: AddParticipantCommand, RemoveParticipantCommand; GetUsersForParticipantQuery.
    - **MeetingAccess**: GetMeetingByAccessTokenQuery (davet linki ile erişim kontrolü).
  - **Mappings**: AutoMapper `MappingProfile` (Entity → DTO; Status/StatusText, GetMeetingStatusDisplay).
- **Paketler**: MediatR, FluentValidation, AutoMapper, EF Core (sadece IQueryable extension için), BCrypt, Microsoft.Extensions.Configuration.Abstractions.

### 3.3 ToplantiApp.Infrastructure

- **Amaç**: Veri erişimi, e-posta, dosya, zamanlanmış işler.
- **İçerik**:
  - **Data**: `AppDbContext`; EF Configurations (Meeting, User, MeetingParticipant, MeetingDocument, MeetingLog); `AuditSaveChangesInterceptor` (CreatedByUserId, UpdatedByUserId, CreatedAt, UpdatedAt — `ICurrentUserProvider` ile).
  - **Repositories**: GenericRepository, MeetingRepository, UserRepository, MeetingParticipantRepository, UnitOfWork.
  - **Services**: TokenService (JWT), MailService (MailKit, Türkçe şablonlar, Türkiye saati), FileService (yükleme, sıkıştırma, indirme).
  - **Jobs**: Quartz ile `CleanupCancelledMeetingsJob` (eski iptal edilmiş toplantıları siler).
- **Paketler**: EF Core SqlServer, MailKit, Quartz, BCrypt, System.IdentityModel.Tokens.Jwt.

### 3.4 ToplantiApp.API

- **Amaç**: HTTP arayüzü, kimlik doğrulama, CORS, Swagger.
- **İçerik**:
  - **Controllers**: AuthController (login, register), MeetingController (CRUD, cancel, delete), MeetingParticipantController (participants), MeetingDocumentController (upload, download), MeetingAccessController (meeting-room by token).
  - **Middleware**: ExceptionMiddleware (AppException, ValidationException, genel hata → JSON response).
  - **Services**: CurrentUserProvider (`ICurrentUserProvider`; JWT NameIdentifier claim’den kullanıcı id).
- **Konfigürasyon**: JWT (Issuer, Audience, Key), CORS (FrontendUrl), Swagger Bearer auth; startup’ta migration ve trigger migration çalıştırılır.

---

## 4. API Özeti

- **Base URL**: `/api`.
- **Auth**: JWT Bearer; çoğu endpoint `[Authorize]`.
- **Auth**: POST `/auth/login`, POST `/auth/register`.
- **Meetings**: GET/POST `/meeting`, GET/PUT/DELETE `/meeting/{id}`, PUT `/meeting/{id}/cancel`; sayfalı liste query parametreleri (pageNumber, pageSize).
- **Participants**: POST/DELETE `/meetings/{meetingId}/participants`, GET search-users.
- **Documents**: POST `/meetings/{meetingId}/documents`, GET `/meetings/{meetingId}/documents/{documentId}` (indirme).
- **Meeting room**: GET `/meeting-room/{accessToken}` (davet linki ile erişim kontrolü).

Response yapısı: `{ success, message, statusCode, data? }`; listeler için `PaginatedResponse` (data, pageNumber, pageSize, totalCount).

---

## 5. Güvenlik ve Audit

- **Şifre**: Kayıtta BCrypt ile hashlenir; girişte doğrulanır.
- **JWT**: Login sonrası token dönülür; API isteklerinde `Authorization: Bearer <token>` beklenir.
- **Mevcut kullanıcı**: Handler’larda `ICurrentUserProvider.GetCurrentUserId()`; audit alanları interceptor ile atanır (request scope’ta).
- **Yetki**: Toplantı işlemlerinde “sadece oluşturan” kontrolü (CreatedByUserId) uygulanır; yetkisiz işlemde ForbiddenException.

---

## 6. Veritabanı ve Migration

- **Provider**: SQL Server (connection string: appsettings).
- **Migration**: EF Core migrations; uygulama başlarken `Migrate()` ve özel trigger migration’lar çalıştırılır.
- **Interceptor**: `AuditSaveChangesInterceptor` ile eklenen/güncellenen `BaseEntity`/`AuditableEntity` kayıtlarına CreatedByUserId, UpdatedByUserId, CreatedAt, UpdatedAt atanır.

---

## 7. E-posta ve Dosya

- **Mail**: MailKit ile SMTP; davet ve bildirim şablonları Türkçe; tarih/saat Türkiye saatine (Europe/Istanbul) çevrilerek gösterilir.
- **Dosya**: Toplantı dökümanları yüklenir, isteğe bağlı sıkıştırılır; indirme endpoint’i blob döner.

---

## 8. Zamanlanmış İş

- **Quartz**: Hosted service olarak çalışır.
- **CleanupCancelledMeetingsJob**: Belirli aralıklarla eski iptal edilmiş toplantıları siler (yapılandırılabilir).

---

## 9. Özet Tablo

| Konu | Detay |
|------|--------|
| Runtime | .NET 9 |
| Mimari | Clean Architecture, CQRS (MediatR) |
| ORM | Entity Framework Core 9, SQL Server |
| Kimlik | JWT Bearer, BCrypt |
| Validasyon | FluentValidation, MediatR pipeline |
| Mapping | AutoMapper |
| Audit | ICurrentUserProvider + SaveChanges interceptor |
| E-posta | MailKit, Türkçe şablonlar |
| Zamanlanmış iş | Quartz.NET |
| Dokümantasyon | Swagger (OpenAPI) |

Bu yapı, SOLID ve DDD prensiplerine uygun, test edilebilir ve bakımı kolay bir backend sunar; frontend ile birlikte toplantı yönetimi, katılımcı/döküman işlemleri ve davet linki ile erişim senaryolarını kapsar.

---

## 10. Projeye Eklenebilecek Yapılar, Mimari ve Özellikler (BE)

Aşağıdakiler, mevcut .NET backend’e eklenebilecek yapılar ve özellikler için fikir listesidir; öncelik iş ihtiyacı ve operasyonel gerekliliklere göre belirlenmelidir.

### 10.1 Önbellekleme (Caching)

- **Redis (veya dağıtık cache):** Sık kullanılan veriler (örn. kullanıcı listesi, toplantı listesi sayfaları) için cache; API yanıt süresini ve veritabanı yükünü azaltır. `IDistributedCache` ile abstract’lanabilir, uygulama katmanı cache’den habersiz kalır.
- **Response caching:** Belirli GET endpoint’lerinde `[ResponseCache]` veya middleware ile kısa süreli HTTP cache (örn. meeting-room/{token} için birkaç saniye).
- **Output cache (ASP.NET Core 7+):** Endpoint bazlı output cache; aynı parametreyle gelen isteklerde cache’den cevap.

### 10.2 Rate limiting ve güvenlik

- **Rate limiting:** Aynı IP veya kullanıcıdan saniye/dakika başına istek sınırı; brute-force ve kötüye kullanımı azaltır. ASP.NET Core 7+ `RateLimiter` middleware veya AspNetCoreRateLimit paketi.
- **API key / scope:** Harici entegrasyonlar için API key ve scope tabanlı yetkilendirme (örn. sadece okuma veya sadece belirli kaynaklar).
- **Audit log genişletme:** Kim, ne zaman, hangi endpoint’e istek attı (ve isteğe bağlı request body hash) gibi detayların ayrı bir audit tablosunda veya dış serviste tutulması; uyumluluk ve güvenlik incelemesi için.

### 10.3 Sağlık kontrolü ve izleme

- **Health checks:** `AddHealthChecks()` ile veritabanı, Redis (varsa), dış servisler (SMTP) için endpoint; `/health` veya `/ready` ile container/orchestrator’ın canlılık kontrolü.
- **Metrikler (OpenTelemetry / Prometheus):** İstek sayısı, süre, hata oranı; dashboard ve uyarı için. Application Insights veya Prometheus exporter ile entegrasyon.
- **Structured logging:** Serilog vb. ile JSON log; seviye, correlation id ve alan bazlı arama; production’da log agregasyon (örn. ELK, Seq) ile kullanım.

### 10.4 API ve sürümleme

- **API versioning:** URL veya header ile sürüm (örn. `/api/v1/meeting`, `/api/v2/meeting`); eski istemciler v1’de kalırken yeni özellikler v2’de sunulabilir.
- **OpenAPI dokümantasyonu:** Swagger’da örnek değerler, açıklamalar ve ortak şemaların iyileştirilmesi; frontend ve harici tüketiciler için net sözleşme.
- **HATEOAS / link’ler (isteğe bağlı):** Response’larda ilgili kaynaklara link; özellikle genel amaçlı API’lerde keşfedilebilirlik artar.

### 10.5 Arka plan işleri ve event’ler

- **Background job çeşitlendirme:** Quartz ile ek job’lar: toplantı hatırlatma e-postaları, rapor üretimi, dış sistem senkronizasyonu. Job’lar Application katmanında interface ile tanımlanıp Infrastructure’da implemente edilebilir (Clean Architecture uyumu).
- **Event-driven / mesaj kuyruğu:** Önemli olaylar (toplantı oluşturuldu, iptal edildi, döküman eklendi) için domain event’ler; RabbitMQ, Azure Service Bus veya Redis Stream ile tüketici servisler (e-posta, bildirim, analitik). Domain katmanında sadece event sözleşmeleri, uygulama detayı Infrastructure’da kalır.
- **Outbox pattern:** Event’leri önce veritabanına “outbox” tablosuna yazıp, arka planda kuyruğa göndermek; transaction tutarlılığı ve mesaj kaybını önleme.

### 10.6 Veri ve raporlama

- **Read model / CQRS genişletme:** Yoğun okuma senaryoları için ayrı okuma modeli (örn. materialized view veya ayrı tablo); listeleme ve raporlama sorguları yazma tarafından ayrılır, performans iyileşir.
- **Export endpoint’leri:** Toplantı listesi veya detayların CSV/Excel olarak indirilmesi; MediatR query ve stream tabanlı yanıt ile bellek kullanımı kontrol altında tutulabilir.
- **Arşivleme:** Eski/iptal toplantıların “soğuk” depolamaya taşınması veya sadece özet bilginin tutulup detayın arşivlenmesi; ana veritabanı boyutunun yönetimi.

### 10.7 Altyapı ve dağıtım

- **Container (Docker):** API ve gerekirse veritabanı/migration için Dockerfile ve docker-compose; tutarlı ortam ve kolay dağıtım.
- **Config ve secret yönetimi:** Hassas bilgilerin environment veya secret store (Azure Key Vault, HashiCorp Vault) ile verilmesi; appsettings’te şifre/token tutulmaması.
- **Multi-tenancy (isteğe bağlı):** Organizasyon/kiracı bazlı veri ayrımı; tenant id’nin request’ten alınıp tüm sorgularda filtrelenmesi; büyük ölçekte çok kiracılı SaaS senaryosu için.

### 10.8 Test ve kalite

- **Unit test:** Application handler’ları, validator’lar, domain kuralları; mock repository ve servisler ile.
- **Integration test:** API projesi ile gerçek HTTP istekleri; test veritabanı veya InMemory provider; auth, CRUD ve hata senaryoları.
- **Contract test:** API sözleşmesinin (request/response şeması) değişmemesini doğrulama; frontend veya harici tüketicilerle uyum için.

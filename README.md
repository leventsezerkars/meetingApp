# Toplanti Yonetim Uygulamasi

Cok katmanli .NET 9 + Angular 20 + MSSQL toplanti yonetim sistemi.

## Teknolojiler

### Backend
- **.NET 9** (C#)
- **Clean Architecture** + **DDD** + **CQRS** (MediatR)
- **Entity Framework Core 9** (Code-First, Fluent API)
- **MSSQL** veritabani
- **JWT Bearer Token** kimlik dogrulama
- **BCrypt** sifre hashleme
- **Quartz.NET** zamanlayici (iptal edilen toplantilarin 30 gun sonra otomatik silinmesi)
- **MSSQL Trigger** silinen toplantilarin loglanmasi
- **MailKit** SMTP e-posta servisi
- **Gzip** dosya sikistirma
- **Swagger/OpenAPI** dokumantasyonu
- **FluentValidation** veri dogrulama

### Frontend
- **Angular 20** (standalone components, signals)
- **Bootstrap 5** (ng-bootstrap)
- **JWT Interceptor** ve **AuthGuard**

### Altyapi
- **Docker** & **docker-compose**

## Proje Yapisi

```
src/
├── ToplantiApp.Domain/          # Entity, Enum, Interface
├── ToplantiApp.Application/     # CQRS Commands/Queries, DTOs, Validators
├── ToplantiApp.Infrastructure/  # DbContext, Repository, Services, Jobs
├── ToplantiApp.API/             # Controllers, Middleware, Program.cs
└── ToplantiApp.Web/             # Angular 20 Frontend
```

## Ozellikler

### Kullanici Yonetimi
- Kayit olma (ad, soyad, email, telefon, sifre, profil resmi)
- Kayit sonrasi hos geldiniz e-postasi
- JWT tabanli giris

### Toplanti Yonetimi
- Toplanti CRUD (ad, aciklama, baslangic/bitis tarihi)
- Dokuman yukleme (gzip sikistirma destegi)
- Toplanti iptal etme
- Iptal edilen toplantilarin 30 gun sonra otomatik silinmesi (Quartz.NET)
- Silinen toplantilarin MSSQL Trigger ile loglanmasi

### Katilimci Yonetimi
- Dahili kullanici ekleme (sistemdeki kullanicilar)
- Harici katilimci ekleme (email ile)
- E-posta ile toplanti daveti gonderme

### Toplanti Odasi
- URL tabanli erisim (AccessToken ile)
- Zaman kontrolu: sadece toplanti saatleri icinde erisim
- Baslamamis/bitmis/iptal edilmis toplantilara erisim engeli

## Calistirma

### Docker ile

```bash
docker-compose up --build
```

Uygulama http://localhost:5000 adresinde calisir.

### Manuel

1. MSSQL Server kurun ve `appsettings.json` icindeki connection string'i guncelleyin

2. API'yi calistirin:
```bash
cd src/ToplantiApp.API
dotnet run
```

3. Angular frontend'i calistirin:
```bash
cd src/ToplantiApp.Web
npm install
ng serve
```

Frontend: http://localhost:4200
API Swagger: http://localhost:5000/swagger

## API Endpointleri

| Yontem | Endpoint | Aciklama |
|--------|----------|----------|
| POST | /api/auth/register | Kayit ol |
| POST | /api/auth/login | Giris yap |
| GET | /api/meeting | Toplantilari listele |
| POST | /api/meeting | Toplanti olustur |
| GET | /api/meeting/{id} | Toplanti detayi |
| PUT | /api/meeting/{id} | Toplanti guncelle |
| PUT | /api/meeting/{id}/cancel | Toplanti iptal et |
| POST | /api/meetings/{id}/participants | Katilimci ekle |
| DELETE | /api/meetings/{id}/participants/{pid} | Katilimci sil |
| POST | /api/meetings/{id}/documents | Dokuman yukle |
| GET | /api/meetings/{id}/documents/{did} | Dokuman indir |
| GET | /api/meeting-room/{token} | Toplanti odasina eris |

## Yapilandirma

`appsettings.json` icinde asagidaki ayarlar yapilandirilabilir:

- **ConnectionStrings:DefaultConnection** - MSSQL baglanti dizesi
- **Jwt:Key** - JWT imza anahtari
- **Mail:Host/Port/Username/Password** - SMTP ayarlari
- **App:FrontendUrl** - Angular frontend URL'i

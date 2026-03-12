# Toplantı Uygulaması — Docker Teknik Dokümanı

Bu doküman, **ToplantiApp** projesinin Docker ile nasıl derlendiğini, çalıştırıldığını ve yapılandırıldığını açıklar. Docker Desktop kullanılarak tek komutla API, Angular SPA ve SQL Server aynı ortamda ayağa kaldırılabilir.

---

## 1. Genel Bakış

| Bileşen | Açıklama |
|---------|----------|
| **Dockerfile** | Çok aşamalı (multi-stage) build: Angular build → .NET API build → runtime imajı. API + Angular tek konteynerde servis edilir. |
| **docker-compose.yml** | Üç servis: **mssql** (SQL Server 2022), **mailpit** (sahte SMTP / mail test), **api** (ASP.NET Core + wwwroot’ta Angular). API, MSSQL healthy olduktan sonra başlar. |
| **.dockerignore** | Build bağlamından gereksiz klasör/dosyaların çıkarılması (bin, obj, node_modules, dist vb.). |

### Port ve Erişim

| Servis | Host portu | Container içi | Erişim URL |
|--------|------------|---------------|-------------|
| API (+ Angular SPA) | 5000 | 8080 | http://localhost:5000 |
| SQL Server | 1433 | 1433 | localhost:1433 (SSMS, connection string) |
| Mailpit (SMTP) | 1025 | 1025 | Uygulama buraya mail gönderir |
| Mailpit (Web UI) | 8025 | 8025 | http://localhost:8025 (gönderilen mailleri görüntüleme) |

---

## 2. Dosya Yapısı ve Görevleri

### 2.1 Dockerfile

Çok aşamalı build; son imaj sadece runtime + yayınlanmış API + Angular çıktısı içerir.

| Aşama | Base imaj | Görev |
|-------|-----------|--------|
| **angular-build** | node:22-alpine | `npm ci` → `ng build --configuration production`. Çıktı: `dist/ToplantiApp.Web/browser`. |
| **api-build** | mcr.microsoft.com/dotnet/sdk:9.0 | Solution restore (`ToplantiCaseApp.slnx`), `dotnet publish` ile API’yi Release’te derleyip `/app/publish` içine alır. |
| **Runtime** | mcr.microsoft.com/dotnet/aspnet:9.0 | API publish çıktısı + Angular’ın `browser` çıktısı `./wwwroot` altına kopyalanır. Kestrel 8080’de dinler. |

Önemli noktalar:

- Solution dosyası: **ToplantiCaseApp.slnx** (proje kökünde).
- Angular proje adı `angular.json` ile uyumlu olmalı; build çıktı yolu: `dist/ToplantiApp.Web/browser`.
- `ASPNETCORE_URLS=http://+:8080` ile Kestrel sadece 8080’i kullanır; docker-compose’da 5000:8080 map edilir.

#### Dockerfile satır satır açıklama

| Satır | İçerik | Açıklama |
|-------|--------|----------|
| 1 | `# Stage 1: Build Angular` | Yorum: Birinci aşama — Angular uygulamasının derlenmesi. |
| 2 | `FROM node:22-alpine AS angular-build` | **Stage 1** başlangıcı. Temel imaj: Node.js 22 (Alpine). Bu aşamaya isim verilir: `angular-build`; ileride `COPY --from=angular-build` ile çıktı alınacak. |
| 3 | `WORKDIR /app/web` | Konteyner içinde çalışma dizinini `/app/web` yapar. Sonraki `COPY` ve `RUN` komutları bu dizinde çalışır. |
| 4 | `COPY src/ToplantiApp.Web/package*.json ./` | Host’taki `package.json` ve `package-lock.json` dosyalarını konteynerin `/app/web` içine kopyalar. Önce sadece bağımlılık dosyaları alınır ki `npm ci` için cache kullanılabilsin. |
| 5 | `RUN npm ci` | `package-lock.json`’a göre bağımlılıkları yükler. `npm install` yerine `npm ci` kullanılır; lock dosyası ile bire bir aynı sürümler kurulur, build tekrarlanabilir olur. |
| 6 | `COPY src/ToplantiApp.Web/ .` | Angular projesinin tüm kaynak kodunu (ts, html, angular.json vb.) `/app/web` içine kopyalar. `node_modules` .dockerignore’da olduğu için gelmez. |
| 7 | `RUN npx ng build --configuration production` | Angular’ı production konfigürasyonu ile derler. Çıktı varsayılan olarak `dist/ToplantiApp.Web/browser` altında oluşur. |
| 8 | (boş) | Stage 1 biter. |
| 9 | `# Stage 2: Build .NET API` | Yorum: İkinci aşama — .NET API’nin derlenmesi. |
| 10 | `FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build` | **Stage 2** başlar. Temel imaj: .NET 9 SDK (build için gerekli). Bu aşama adı: `api-build`. Önceki stage’in dosyaları burada yok; sadece bu imajdan devam edilir. |
| 11 | `WORKDIR /app` | Konteyner içi çalışma dizini `/app`. |
| 12 | `COPY ToplantiCaseApp.slnx ./` | Solution dosyasını proje kökünden `/app` içine kopyalar. `dotnet restore` bu dosyayı kullanacak. |
| 13–16 | `COPY src/.../*.csproj ...` | Her proje klasöründeki `.csproj` dosyalarını ilgili dizin yapısıyla kopyalar (Domain, Application, Infrastructure, API). Önce yalnızca proje dosyaları alınır; böylece `dotnet restore` katmanı cache’lenebilir, kaynak kodu değişmeden restore tekrar kullanılır. |
| 17 | `RUN dotnet restore ToplantiCaseApp.slnx` | Solution’a ait NuGet paketlerini indirir ve proje grafiğini hazırlar. Bağımlılıklar bu aşamada çözülür. |
| 18 | `COPY src/ src/` | Tüm `src/` altındaki kaynak kodu (kod, config vb.) konteynere kopyalar. Restore zaten yapıldığı için sadece kaynak değişince bu ve sonraki adım yeniden çalışır. |
| 19 | `RUN dotnet publish ... -c Release -o /app/publish` | API projesini Release modunda yayınlar. Çıktı `/app/publish` dizinine yazılır (DLL’ler, bağımlılıklar, config). Bu dizin sonra runtime stage’e kopyalanacak. |
| 20 | (boş) | Stage 2 biter. |
| 21 | `# Stage 3: Runtime` | Yorum: Üçüncü aşama — sadece çalışma zamanı; SDK yok, sadece çalışan uygulama. |
| 22 | `FROM mcr.microsoft.com/dotnet/aspnet:9.0` | **Stage 3** (final imaj). Temel: .NET 9 ASP.NET Core runtime. SDK ve kaynak kodu yok; sadece çalıştırmak için gerekli kütüphaneler. İmaj boyutu küçük kalır. |
| 23 | `WORKDIR /app` | Çalışma dizini `/app`. Uygulama buradan çalışacak. |
| 24 | `COPY --from=api-build /app/publish .` | Stage 2’deki `api-build` aşamasında üretilen `/app/publish` içeriğini bu imaja `/app` altına kopyalar. Yani yayınlanmış API (ToplantiApp.API.dll vb.) burada. |
| 25 | `COPY --from=angular-build /app/web/dist/ToplantiApp.Web/browser ./wwwroot` | Stage 1’deki Angular build çıktısını (`dist/ToplantiApp.Web/browser`) bu imajda `./wwwroot` dizinine kopyalar. ASP.NET Core `UseStaticFiles()` ile buradan dosya sunar; böylece Angular SPA tek porttan servis edilir. |
| 26 | (boş) | Kopyalama biter. |
| 27 | `EXPOSE 8080` | Konteynerin 8080 portunu dinlediğini belge amaçlı bildirir. Gerçek dinleme `ENV ASPNETCORE_URLS` ve uygulama ile yapılır; `EXPOSE` dokümantasyon ve docker-compose port eşlemesi için. |
| 28 | `ENV ASPNETCORE_URLS=http://+:8080` | Kestrel’in tüm arayüzlerde (`+`) 8080 portunda dinlemesini sağlar. Sadece `localhost` değil `0.0.0.0` olur; böylece konteyner dışından (host’tan) erişilebilir. |
| 29 | `ENTRYPOINT ["dotnet", "ToplantiApp.API.dll"]` | Konteyner başlarken çalışacak komut: `dotnet ToplantiApp.API.dll`. Bu process çalıştığı sürece konteyner ayakta kalır. |

### 2.2 docker-compose.yml

**Servisler:**

1. **mssql**
   - Görüntü: `mcr.microsoft.com/mssql/server:2022-latest`
   - Ortam: `ACCEPT_EULA=Y`, `SA_PASSWORD`, `MSSQL_PID=Express`
   - Veri: `mssql_data` volume → `/var/opt/mssql`
   - **healthcheck**: `sqlcmd` ile `SELECT 1`; healthy olana kadar API başlamaz.

2. **mailpit**
   - Görüntü: `axllent/mailpit`. Sahte SMTP sunucusu; uygulama gönderdiği tüm mailleri yakalar, gerçek dışarı çıkmaz. Kendi mail bilgilerinizi vermenize gerek yok.
   - **FROM adresi:** Toplantı daveti/iptal maillerinde **gönderen (From)** DB’deki organizatörün e-postası (`meeting.CreatedBy.Email`) kullanılır; hoş geldin mailinde `Mail__From` (örn. noreply@toplanti.app) kullanılır. Mailpit her FROM’u kabul eder.
   - **Alıcı (To):** Uygulama istediğiniz adrese (DB’den gelen katılımcı e-postası vb.) “gönderir”; mail Mailpit’te görünür, gerçek posta kutusuna gitmez.
   - **1025**: SMTP portu — API `Mail__Host=mailpit`, `Mail__Port=1025` ile buraya bağlanır.
   - **8025**: Web arayüzü — http://localhost:8025 ile gönderilen mailleri görüntüleyebilirsiniz.
   - TLS ve kullanıcı adı/şifre yok; sadece geliştirme/test için.

3. **api**
   - Build: proje kökündeki `Dockerfile` (`.`)
   - `restart: on-failure`: Veritabanı gecikmeli açılsa bile yeniden dener.
   - `depends_on.mssql.condition: service_healthy`: MSSQL healthy olmadan API başlamaz.
   - Ortam değişkenleri: Connection string (Server=mssql), JWT ayarları, `App__FrontendUrl=http://localhost:5000`, **Mail** (Host=mailpit, Port=1025, SecureSocketOptions=None, From=noreply@toplanti.app; Username/Password boş).

**Volume:**

- `mssql_data`: SQL Server verileri kalıcı; `docker compose down` sonrası veri silinmez (volume silinmediği sürece).

### 2.3 .dockerignore

Build bağlamına dahil edilmemesi gerekenler:

```
**/bin
**/obj
**/node_modules
**/dist
**/.angular
**/.vs
**/.vscode
**/.git
*.md
.gitignore
.dockerignore
```

Böylece build süresi kısalır ve gereksiz dosyalar imaja girmez.

---

## 3. Ortam Değişkenleri (API)

docker-compose içinde API için kullanılan değişkenler:

| Değişken | Açıklama | Örnek / Not |
|----------|----------|------------------|
| `ASPNETCORE_ENVIRONMENT` | Ortam adı | Production |
| `ConnectionStrings__DefaultConnection` | SQL Server bağlantı dizesi | Server=mssql;Database=ToplantiCaseDb;User Id=sa;Password=...;TrustServerCertificate=True; |
| `Jwt__Key` | JWT imza anahtarı | Uzun, güvenli bir değer |
| `Jwt__Issuer` / `Jwt__Audience` | JWT issuer/audience | ToplantiApp |
| `Jwt__ExpireHours` | Token geçerlilik süresi (saat) | 24 |
| `App__FrontendUrl` | CORS / frontend kök adresi | http://localhost:5000 |
| `Mail__Host` | SMTP sunucusu (Docker’da: mailpit) | mailpit |
| `Mail__Port` | SMTP portu | 1025 (Mailpit) / 587 (gerçek SMTP) |
| `Mail__SecureSocketOptions` | TLS: `StartTls` veya `None` (Mailpit için None) | None |
| `Mail__From` | Gönderen adresi (görünen) | noreply@toplanti.app |
| `Mail__Username` / `Mail__Password` | SMTP kimlik (Mailpit’te boş bırakılır) | — |
| `TZ` | Konteyner saat dilimi (API tarafında `DateTime.Now` vb. için) | `Europe/Istanbul` (GMT+3). Değiştirmek için [IANA timezone](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) adı kullanın (örn. `America/New_York`). |

Şifre ve JWT key’i production’da güvenli bir yöntemle (örn. secret/store) yönetilmelidir.

---

## 4. Projeyi Docker ile Çalıştırma Adımları

**Ön koşul:** Docker Desktop yüklü ve çalışıyor olmalı.

### Adım 1: Proje köküne geçin

```powershell
cd "d:\Dosyalar\Projeler\Kendi Projelerim\Alpata_ToplantiCaseApp"
```

### Adım 2: Docker’ın çalıştığını doğrulayın

```powershell
docker info
```

Hata almıyorsanız Docker hazırdır.

### Adım 3: (İsteğe bağlı) Eski konteyner ve imajları temizleyin

```powershell
docker compose down
docker system prune -f
```

**Migration’ları sıfırladıysanız veya “Veritabani migration basarisiz” hatası alıyorsanız:** Eski veritabanı ve migration geçmişi volume’da kalmış olabilir. Volume’ları da silip veritabanını sıfırdan oluşturun:

```powershell
docker compose down -v
docker compose up --build
```

(`-v` ile `mssql_data` volume silinir; ilk açılışta tek migration temiz uygulanır.)

### Adım 4: Build ve çalıştırma

```powershell
docker compose up --build
```

- `--build`: API imajını Dockerfile ile yeniden derler.
- İlk seferde Angular ve .NET build’leri nedeniyle birkaç dakika sürebilir.
- Önce **mssql** ayağa kalkar; healthcheck “healthy” olunca **api** başlar.

### Adım 5: Uygulamaya erişim

- **Tarayıcı:** http://localhost:5000  
  (Angular SPA, API’nin `wwwroot` üzerinden servis edilir.)
- **Swagger:** Production’da Program.cs’e göre kapalı olabilir; gerekirse ortam veya kod ile açılabilir.

### Adım 6: Durdurma

Terminalde:

```text
Ctrl + C
```

Ardından konteynerları kaldırmak için:

```powershell
docker compose down
```

### Arka planda çalıştırma

```powershell
docker compose up --build -d
```

Logları takip etmek:

```powershell
docker compose logs -f
```

Durdurmak:

```powershell
docker compose down
```

---

## 5. Sık Karşılaşılan Sorunlar ve Çözümleri

| Sorun | Olası neden | Çözüm |
|--------|----------------|--------|
| Port 5000 veya 1433 kullanımda | Başka bir uygulama aynı portu kullanıyor | `docker-compose.yml` içinde `ports` bölümünde farklı port kullanın (örn. `"5001:8080"`). |
| MSSQL healthcheck sürekli başarısız | Bazı 2022 imajlarında `sqlcmd` yolu yok veya farklı | Healthcheck bloğunu geçici kaldırın veya imajı sabitleyin: `mcr.microsoft.com/mssql/server:2022-cu16-ubuntu-22.04`. API yine `depends_on: mssql` ile bekler; gerekirse `restart: on-failure` sayesinde birkaç denemede bağlanır. |
| API “connection refused” / migration hatası | API, SQL Server tam açılmadan başlamış | Birkaç saniye bekleyip `docker compose restart api` deneyin veya `docker compose up --build` ile yeniden başlatın. |
| localhost:5000 açılmıyor | API konteyneri migration/trigger hatasında çöküyor olabilir | `docker compose ps` ve `docker compose logs api` ile durum ve hata mesajını inceleyin. |
| **Veritabani migration basarisiz. Uygulama kapaniyor.** | Eski migration geçmişi volume’da kalmış; tablolar zaten var veya migration ID uyumsuz | `docker compose down -v` ile volume’ları silin, ardından `docker compose up --build`. Log’daki “Hata: …” satırına bakarak da nedeni görebilirsiniz. |
| Angular sayfası 404 veya boş | wwwroot’a Angular çıktısı gelmemiş | Dockerfile’daki path’in `dist/ToplantiApp.Web/browser` olduğundan ve `angular.json`’daki proje adının `ToplantiApp.Web` ile uyumlu olduğundan emin olun. |
| Build sırasında “solution not found” | Solution dosyası adı/konumu | Kök dizinde `ToplantiCaseApp.slnx` dosyasının bulunduğundan emin olun; Dockerfile’da aynı isim kullanılır. |

---

## 6. Yapılan Düzeltmeler (Doküman Güncellemesi Sırasında)

- **Dockerfile:** Angular build çıktı yolu `dist/toplanti-app.web/browser` → `dist/ToplantiApp.Web/browser` olarak düzeltildi (`angular.json` proje adı ile uyumlu).
- **docker-compose.yml:** MSSQL için healthcheck eklendi; API `depends_on.mssql.condition: service_healthy` ile MSSQL hazır olana kadar bekliyor.
- **docker-compose.yml:** API’ye `restart: on-failure` eklendi; veritabanı gecikmeli açılsa bile API tekrar deneyecek.

---

## 7. Özet Komutlar

| Amaç | Komut |
|------|--------|
| İlk kez veya değişiklik sonrası çalıştırma | `docker compose up --build` |
| Arka planda çalıştırma | `docker compose up --build -d` |
| Logları izleme | `docker compose logs -f` |
| Durdurma | `Ctrl+C` ardından `docker compose down` |
| Sadece durdurma (arka planda) | `docker compose down` |
| Volume’lar dahil tam temizlik | `docker compose down -v` (veritabanı verisi de silinir) |

Bu doküman, DOC_BE.md (backend) ve DOC_FE.md (frontend) ile birlikte projenin yerel ve Docker tabanlı çalıştırma senaryolarını tamamlar.

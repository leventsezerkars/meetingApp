# Toplantı Uygulaması — Frontend Teknik Dokümanı

Bu doküman, **ToplantiApp.Web** (Angular) frontend projesinin kullandığı teknolojileri, mimari yapıyı, proje içeriğini ve teknik detayları açıklar.

---

## 1. Kullanılan Teknolojiler

| Teknoloji | Sürüm | Amaç |
|-----------|--------|------|
| **Angular** | 20.3.x | SPA framework, standalone component yapısı |
| **TypeScript** | 5.9.x | Tip güvenliği, derleme |
| **RxJS** | 7.8.x | Asenkron akışlar, HTTP ve state reaktif kullanımı |
| **Bootstrap** | 5.3.x | Grid, bileşenler, responsive UI |
| **ng-bootstrap** | 19.x | Angular uyumlu Bootstrap bileşenleri |
| **ngx-toastr** | 19.x | Bildirim (toast) mesajları |
| **Moment.js** | 2.30.x | Tarih/saat formatlama ve işlemler |
| **Zone.js** | 0.15.x | Angular change detection (varsayılan) |
| **Angular Router** | 20.3.x | Sayfa yönlendirme, lazy loading |
| **Angular HttpClient** | 20.3.x | REST API iletişimi |
| **Angular Forms** | 20.3.x | Reactive/FormsModule ile form yönetimi |

### Build & Geliştirme

- **Angular CLI** 20.3.x (`ng build`, `ng serve`)
- **Prettier**: Kod formatı (single quote, printWidth 100, HTML için angular parser)
- **Karma + Jasmine**: Birim test altyapısı (projede test dosyaları skip edilmiş)

---

## 2. Proje Dizin Yapısı

```
ToplantiApp.Web/
├── src/
│   ├── index.html              # Ana HTML, <app-root>
│   ├── main.ts                 # Bootstrap: bootstrapApplication(AppComponent, appConfig)
│   ├── styles.css              # Global stiller (Bootstrap + özel)
│   ├── environments/
│   │   ├── environment.ts       # development: apiUrl
│   │   └── environment.prod.ts  # production: apiUrl
│   └── app/
│       ├── app.ts              # Kök component (router-outlet, navbar, toast)
│       ├── app.config.ts       # Providers: router, httpClient, toastr, auth interceptor
│       ├── app.routes.ts        # Route tanımları, lazy loading, authGuard
│       ├── core/                # Singleton servisler, guard, interceptor, modeller
│       │   ├── guards/
│       │   │   └── auth.guard.ts
│       │   ├── interceptors/
│       │   │   └── auth.interceptor.ts
│       │   ├── models/
│       │   │   ├── auth.model.ts
│       │   │   ├── meeting.model.ts
│       │   │   └── response.model.ts
│       │   ├── services/
│       │   │   ├── auth.service.ts
│       │   │   ├── meeting.service.ts
│       │   │   └── toast.service.ts
│       │   └── utils/
│       │       ├── api-error.utils.ts
│       │       └── date.utils.ts
│       └── features/           # Özellik bazlı sayfalar
│           ├── auth/
│           │   ├── login.component.ts / .html
│           │   └── register.component.ts / .html
│           ├── meetings/
│           │   ├── meeting-list.component.ts / .html
│           │   ├── meeting-create.component.ts / .html
│           │   ├── meeting-detail.component.ts / .html
│           │   ├── meeting-edit.component.ts / .html
│           └── meeting-room/
│               └── meeting-room.component.ts / .html
├── public/                     # Statik varlıklar (build’e kopyalanır)
├── angular.json
├── package.json
├── tsconfig.json
├── tsconfig.app.json
└── tsconfig.spec.json
```

---

## 3. Uygulama Giriş Noktası ve Konfigürasyon

### 3.1 `main.ts`

- `bootstrapApplication(AppComponent, appConfig)` ile uygulama başlatılır.
- `appConfig` ile router, HttpClient, interceptor, animasyon ve toastr sağlanır.

### 3.2 `app.config.ts`

- **provideRouter(routes)**: Route yapılandırması.
- **provideHttpClient(withInterceptors([authInterceptor]))**: Tüm HTTP isteklerine JWT ekleyen interceptor.
- **provideAnimations()**: ngx-toastr ve diğer animasyonlar için.
- **provideToastr(...)**: Süre 4sn, sağ üst, progress bar, kapatma butonu.

### 3.3 `app.ts` (Kök bileşen)

- **RouterOutlet**: Alt route’ların render edildiği alan.
- **Navbar**: Logo, “Toplantılar”, “Yeni Toplantı”; giriş yapılmışsa “Çıkış”.
- **RouterLink / RouterLinkActive**: Menü linkleri ve aktif sınıf.
- **ToastContainerDirective**: ngx-toastr için container.

---

## 4. Routing ve Lazy Loading

### 4.1 Route Tablosu

| Path | Guard | Bileşen | Açıklama |
|------|--------|---------|----------|
| `''` | — | — | `/meetings`e yönlendirme |
| `login` | — | LoginComponent | Giriş sayfası |
| `register` | — | RegisterComponent | Kayıt sayfası |
| `meetings` | authGuard | MeetingListComponent | Toplantı listesi |
| `meetings/create` | authGuard | MeetingCreateComponent | Yeni toplantı |
| `meetings/:id` | authGuard | MeetingDetailComponent | Toplantı detayı |
| `meetings/:id/edit` | authGuard | MeetingEditComponent | Toplantı düzenleme |
| `meeting-room/:accessToken` | — | MeetingRoomComponent | Davet linki ile oda (token ile erişim) |
| `**` | — | — | `/meetings`e fallback |

### 4.2 Lazy Loading

- Tüm feature bileşenleri `loadComponent: () => import('...').then(m => m.XxxComponent)` ile yüklenir.
- Sadece ilgili route’a girildiğinde ilgili chunk indirilir; initial bundle küçük kalır.

### 4.3 Auth Guard

- **authGuard** (functional guard): `AuthService.isLoggedIn()` true ise route açılır, değilse `/login`e yönlendirilir.
- `AuthService` ve `Router` `inject()` ile alınır.

---

## 5. State ve Kimlik Yönetimi

### 5.1 AuthService

- **Signals**: `currentUser`, `token` (private signal); dışarıya `user` (readonly) ve `isLoggedIn` (computed) verilir.
- **Kalıcılık**: Token ve kullanıcı bilgisi `localStorage`’da saklanır; uygulama açılışında `loadFromStorage()` ile okunur.
- **login(data)**: API’ye POST, gelen token ve user ile `handleAuth()`; token/user signal ve localStorage güncellenir.
- **register(formData)**: FormData ile kayıt, sonuç yine `handleAuth()` ile işlenir.
- **logout()**: localStorage temizlenir, signal’lar null yapılır, `/login`e yönlendirilir.
- **getToken()**: Interceptor’da kullanılmak üzere mevcut JWT.

### 5.2 Auth Interceptor

- **authInterceptor** (HttpInterceptorFn): Her istekte `AuthService.getToken()` ile JWT alınır; varsa `Authorization: Bearer <token>` header’ı eklenir.

---

## 6. API İletişimi ve Modeller

### 6.1 Base URL

- `environment.apiUrl` (örn. development: `https://localhost:7220/api`).
- Auth: `${apiUrl}/auth`; Meeting CRUD: `${apiUrl}/meeting`; Alt kaynaklar: `${apiUrl}/meetings/:id/...`.

### 6.2 Response Modelleri (`response.model.ts`)

- **ApiResponse<T>**: `success`, `message`, `statusCode`, `data?: T`.
- **PaginatedResponse<T>**: `data`, `pageNumber`, `pageSize`, `totalCount`, sayfalama alanları.

### 6.3 Meeting Modelleri (`meeting.model.ts`)

- **CreateMeetingDto / UpdateMeetingDto**: name, description?, startDate, endDate.
- **MeetingDto**: id, name, description, startDate, endDate, status (number), statusText, cancelledAt, accessToken, meetingUrl, createdBy (UserDto), participants, documents, createdAt.
- **MeetingListDto**: Liste satırı için: id, name, startDate, endDate, status, statusText, participantCount, createdByName, createdAt.
- **AddParticipantDto**: userId?, email?, fullName? (dahili/harici katılımcı).
- **ParticipantDto**, **MeetingDocumentDto**, **MeetingRoomDto**, **MeetingAccessResult**: Detay ve meeting-room ekranları için.

### 6.4 Auth Modelleri (`auth.model.ts`)

- **LoginDto**, **AuthResponse**, **UserDto**: Giriş/kayıt ve oturum bilgisi.

---

## 7. Servisler

### 7.1 MeetingService

- **getAll(pageNumber, pageSize)**: Sayfalı toplantı listesi.
- **getById(id)**: Tekil toplantı detayı.
- **create(data)**, **update(id, data)**: Oluşturma ve güncelleme.
- **cancel(id)**, **delete(id)**: İptal ve silme.
- **addParticipant(meetingId, data)**, **removeParticipant(meetingId, participantId)**.
- **searchUsers(term)**: Katılımcı eklerken kullanıcı arama.
- **uploadDocument(meetingId, file, compress)**: FormData ile döküman yükleme.
- **downloadDocument(meetingId, documentId)**: Blob olarak indirme.
- **getMeetingRoom(accessToken)**: Davet linki ile oda erişim kontrolü ve meeting bilgisi.

Tüm HTTP çağrıları `Observable` döner; bileşenlerde `subscribe` veya `async` pipe kullanılır.

### 7.2 AuthService

- Yukarıda “State ve Kimlik Yönetimi” altında özetlendi.

### 7.3 ToastService

- Muhtemelen ngx-toastr sarmalayıcısı; başarı/hata mesajları için kullanılır.

---

## 8. Bileşenler (Features)

### 8.1 Ortak Özellikler

- **Standalone**: Tüm bileşenler `standalone: true`.
- **Template**: `templateUrl` ile ayrı `.html` dosyaları kullanılır.
- **Imports**: Gerekli modüller (CommonModule, FormsModule, RouterLink vb.) doğrudan bileşen `imports` dizisinde.

### 8.2 Auth

- **LoginComponent**: Email/şifre formu, `AuthService.login()`, hata durumunda toast; başarıda ana sayfaya yönlendirme.
- **RegisterComponent**: Ad, soyad, email, şifre (FormData ile kayıt), `AuthService.register()`.

### 8.3 Meetings

- **MeetingListComponent**: Sayfalı liste (tablo), filtre (arama), status badge (status === 0 yeşil, değilse kırmızı), statusText gösterimi; Detay / Düzenle / İptal / Sil (yetkiye göre); “Yeni Toplantı” butonu. `@for` ile satır döngüsü, `track m.id`.
- **MeetingCreateComponent**: Toplantı adı, açıklama, başlangıç/bitiş tarih-saat; form validasyonu, `MeetingService.create()`.
- **MeetingDetailComponent**: Toplantı bilgisi, katılımcı listesi, döküman listesi; Düzenle / İptal (status === 0 ise); katılımcı ekleme/çıkarma, döküman yükleme/indirme.
- **MeetingEditComponent**: Mevcut toplantı verisi ile doldurulmuş form, `MeetingService.update()`.

### 8.4 Meeting Room

- **MeetingRoomComponent**: `accessToken` ile route’dan alınır; `getMeetingRoom(accessToken)` ile erişim kontrolü. Erişilebilir değilse mesaj/tarih gösterimi; erişilebilir ise toplantı bilgisi ve içerik (katılımcılar, dökümanlar vb.). Giriş zorunlu değil (davet linki ile misafir erişimi).

---

## 9. Stil ve UI

- **Bootstrap 5**: `angular.json` içinde `bootstrap.min.css` global style olarak eklenir.
- **ngx-toastr**: `toastr.css` global style.
- **styles.css**: Arka plan (#f8f9fa), card border-radius, navbar-brand font ağırlığı gibi proje özel stilleri.
- **Responsive**: Bootstrap grid ve bileşenleri ile mobil uyum.

---

## 10. Yardımcı Araçlar

### 10.1 date.utils

- Tarih/saat formatlama veya API’den gelen tarihleri görüntüleme için kullanılan fonksiyonlar (muhtemelen moment veya Date pipe ile birlikte).

### 10.2 api-error.utils

- HTTP hata cevaplarını (success: false, message, statusCode) parse edip kullanıcıya anlamlı mesaj veya toast göstermek için.

---

## 11. Build ve Ortamlar

- **development**: `ng serve`, `environment.ts` (apiUrl: https://localhost:7220/api), source map açık.
- **production**: `ng build`, `environment.prod.ts` (production apiUrl), output hashing, budget (initial 1MB, component style 8kB).
- **Assets**: `public` klasörü build çıktısına kopyalanır.

---

## 12. Özet Tablo

| Konu | Detay |
|------|--------|
| Framework | Angular 20, standalone components |
| Dil | TypeScript 5.9 |
| UI | Bootstrap 5 + ng-bootstrap, ngx-toastr |
| State (auth) | Signals + localStorage |
| HTTP | HttpClient + auth interceptor |
| Routing | Lazy-loaded, functional authGuard |
| Formlar | FormsModule (template-driven) |
| Tarih | Moment.js + date utils |
| API base | environment.apiUrl |

Bu yapı, tek sayfa uygulaması (SPA) olarak toplantı oluşturma, düzenleme, listeleme, katılımcı/döküman yönetimi ve davet linki ile meeting room erişimini kapsar; kimlik yönetimi ve API iletişimi merkezi ve tutarlı şekilde yapılandırılmıştır.

---

## 13. Angular Terimleri ve Kavramlar

Bu bölümde projede kullanılan Angular ve ilgili terimlerin ne anlama geldiği ve neden kullanıldığı kısaca açıklanır.

### 13.1 Signals

- **Ne:** Angular 16+ ile gelen, reaktif state tutmak için hafif bir primitif. `signal()`, `computed()`, `effect()` ile kullanılır.
- **Projede kullanım:** AuthService’te `currentUser` ve `token` signal ile tutuluyor; `isLoggedIn` bir `computed` signal. Değer değişince sadece bu değerlere bağlı şablonlar güncellenir.
- **Neden:** Geleneksel RxJS `BehaviorSubject` yerine daha basit API, otomatik change detection entegrasyonu ve daha az bellek kullanımı. Şablonlarda `user()` veya `isLoggedIn()` ile okunur.

### 13.2 Observable

- **Ne:** RxJS’in temel tipi; zaman içinde gelen (veya tek seferlik) veri akışını temsil eder. “Lazy”: sadece **subscribe** edildiğinde çalışır.
- **Projede kullanım:** HttpClient tüm metodları `Observable<T>` döner (örn. `getAll()`, `login()`). API cevabı “gelecekte” geldiği için asenkron akış Observable ile modellenir.
- **Neden:** İptal edilebilir (unsubscribe), birleştirilebilir (pipe, mergeMap), tekrar kullanılabilir (paylaşılan stream). Promise’e göre çoklu değer ve iptal senaryolarına daha uygun.

### 13.3 Subscribe

- **Ne:** Bir Observable’ı “dinlemeye” başlamak. `observable.subscribe(next => ..., error => ..., complete => ...)` ile akışa abone olunur; veri gelince callback’ler çalışır.
- **Projede kullanım:** Bileşenlerde `meetingService.getById(id).subscribe({ next: (res) => { ... }, error: (err) => { ... } })` gibi. Manuel subscribe ettiğiniz yerde, bileşen destroy olurken **unsubscribe** (örn. takeUntilDestroyed, AsyncPipe) yapmak gerekir; aksi halde memory leak riski vardır.
- **Alternatif:** Mümkün olduğunda şablonda `async` pipe kullanmak; pipe aboneliği bileşen destroy’da otomatik kaldırır.

### 13.4 Pipe

- **İki anlamı var:**
  1. **RxJS pipe:** `observable.pipe(operator1(), operator2())` — akışı dönüştürür (map, catchError, debounceTime vb.). Veriyi işleyip yeni bir Observable döner.
  2. **Angular pipe (şablonda):** `{{ deger | date:'short' }}` — şablonda gösterimi biçimlendirir (DatePipe, CurrencyPipe, custom pipe’lar).
- **Projede kullanım:** HTTP hatalarında `pipe(tap(...), catchError(...))`; listelerde `| date` veya özel formatlama pipe’ları. AsyncPipe: `observable | async` ile sonucu şablonda göstermek ve aboneliği yönetmek.

### 13.5 Interceptor

- **Ne:** HttpClient’tan çıkan her istek veya gelen her cevap, bir fonksiyon zincirinden (interceptor) geçer. İstekte header ekleme, cevapta hata dönüştürme yapılabilir.
- **Projede kullanım:** `authInterceptor`: Her istekte JWT token’ı `Authorization: Bearer <token>` olarak ekler. Böylece her servis çağrısında token’ı manuel göndermek gerekmez.
- **Neden:** Merkezi, tek yerden tüm isteklerin davranışını kontrol etmek (auth, logging, hata yönetimi).

### 13.6 Guard

- **Ne:** Route’a girilmeden önce çalışan kontrol. `CanActivateFn` true dönerse route açılır, false’da (veya UrlTree) yönlendirme yapılır.
- **Projede kullanım:** `authGuard` — giriş yapılmamışsa `/login`e yönlendirir. Böylece korumalı sayfalar (meetings, create, detail, edit) sadece giriş yapmış kullanıcıya açılır.
- **Neden:** Yetkisiz erişimi tek bir yerde (guard) toplamak; her bileşende “giriş var mı?” kontrolü yazmamak.

### 13.7 Standalone component

- **Ne:** NgModule’a ihtiyaç duymadan, kendi `imports` dizisinde gerekli modülleri/ bileşenleri tanımlayan component. Angular 14+ ile önerilen varsayılan yapı.
- **Projede kullanım:** Tüm bileşenler `standalone: true`; CommonModule, FormsModule, RouterLink vb. doğrudan component’in `imports`’unda.
- **Neden:** Daha az boilerplate, lazy loading ile uyumlu, modül karmaşasını azaltır.

### 13.8 Lazy loading

- **Ne:** Route’a ilk kez girildiğinde ilgili bileşenin (ve bağımlılıklarının) JS chunk’ı indirilir. `loadComponent: () => import('...').then(m => m.XxxComponent)` ile tanımlanır.
- **Projede kullanım:** Tüm feature sayfaları (login, register, meetings, meeting-room) lazy; sadece tıklanan route’un kodu yüklenir.
- **Neden:** İlk açılışta indirilen bundle küçülür (initial load hızlanır); kullanılmayan ekranların kodu hiç yüklenmeyebilir.

### 13.9 Neden SPA (Single Page Application) şeklinde kurgulandı?

- **SPA ne demek:** Uygulama tek bir HTML sayfası (index.html) üzerinden sunulur; sayfa “yenilenmeden” route değişir, içerik JavaScript ile güncellenir. Sunucu her tıklamada yeni HTML sayfası döndürmez.
- **Neden tercih edildi:**
  - **Akıcı UX:** Sayfa geçişleri anında; sunucu round-trip’i sadece API çağrılarında.
  - **Backend ayrımı:** API (REST/JSON) tek bir kez yazılır; aynı API mobil veya farklı istemcilerle de kullanılabilir.
  - **Frontend tek stack:** Tüm UI mantığı (state, routing, form) tek dil/framework (TypeScript/Angular) ile; ekip ve tooling tek odakta.
  - **Offline / PWA potansiyeli:** İleride service worker ile cache, offline destek eklenebilir.
- **Alternatifler:**
  - **MPA (Multi-Page Application):** Her ekran ayrı sunucu sayfası (ör. Razor, PHP). Her tıklamada full sayfa yüklenir; SEO ve ilk render kolay, ama sayfa geçişleri daha ağır ve state yönetimi dağınık olabilir.
  - **SSR (Server-Side Rendering):** Angular SSR veya Next.js gibi; ilk HTML sunucuda üretilir (SEO, ilk ekran hızı). Proje şu an tamamen client-side; ileride SEO veya ilk yükleme kritikse SSR eklenebilir.
  - **Hybrid / Island architecture:** Sayfanın büyük kısmı sunucu, belirli widget’lar SPA (ör. micro frontend). Daha karmaşık; büyük ve modüler ürünlerde düşünülebilir.

Bu projede ihtiyaçlar (hızlı sayfa geçişi, API odaklı backend, tek frontend ekibi) SPA ile iyi örtüşmektedir.

---

## 14. Projeye Eklenebilecek Yapılar, Mimari ve Özellikler (FE)

Aşağıdakiler, mevcut Angular frontend’e eklenebilecek yapılar ve özellikler için fikir listesidir; öncelik proje ihtiyacına göre belirlenmelidir.

### 14.1 State yönetimi

- **NgRx veya Akita:** Toplantı listesi, kullanıcı tercihleri, UI state’i merkezi store’da tutulabilir; sayfa geçişlerinde veri kaybı azalır, ön bellek (cache) ve tekrarsız istek stratejisi kurulabilir.
- **Signals ile basit store:** Büyük refactor istemiyorsanız, mevcut Signals kullanımı genişletilerek feature bazlı “mini store” servisleri (örn. MeetingListStore) yazılabilir.

### 14.2 Form ve validasyon

- **Reactive Forms + kendi validators:** Şu an template-driven form kullanılıyor; karmaşık formlarda Reactive Forms ve özel validator’lar (örn. tarih aralığı, çakışma kontrolü) eklenebilir.
- **Form state persistence:** Taslak (draft) kaydetme; sayfa yenilense veya kullanıcı geri gelse bile form dolu kalabilir (localStorage/sessionStorage veya API taslak endpoint’i).

### 14.3 Performans ve UX

- **Virtual scrolling:** Çok uzun toplantı/katılımcı listelerinde CDK veya ngb özel bileşen ile sadece görünen satırların render edilmesi.
- **Skeleton / loading state:** Veri gelirken anlamlı placeholder (skeleton) veya loading bileşenleri; boş ekran yerine yapıyı göstermek.
- **OnPush change detection:** Bileşenlerde `changeDetection: ChangeDetectionStrategy.OnPush` ve mümkünse immutable veri / Signals ile gereksiz kontrol sayısını azaltmak.
- **PWA / Service Worker:** Offline sayfa, bildirim, cache-first stratejisi ile bazı API/liste sonuçlarının offline kullanımı.

### 14.4 Test ve kalite

- **Unit test (Jasmine/Karma):** Servisler (Auth, Meeting), guard, interceptor, pipe’lar için birim testler; CI’da `ng test` ile çalıştırılabilir.
- **E2E (Cypress/Playwright):** Giriş → toplantı listesi → oluşturma/düzenleme akışları için uçtan uca testler.
- **ESLint + stricter TypeScript:** Kurallar sıkılaştırılarak tip güvenliği ve tutarlı kod stili artırılabilir.

### 14.5 Erişilebilirlik ve uluslararasılaştırma

- **i18n (ngx-translate veya Angular i18n):** Çoklu dil; metinlerin key’ler üzerinden yönetilmesi ve Türkçe/İngilizce (veya daha fazla) dil seçeneği.
- **A11y:** ARIA etiketleri, klavye navigasyonu, odak yönetimi; gerekirse erişilebilir bileşen kütüphanesi (örn. CDK a11y) kullanımı.

### 14.6 Mimari / modülleşme

- **Feature modülleri (lazy):** Zaten route bazlı lazy var; her feature için ortak bileşenler (liste, form, detay) tek bir “feature modül” altında toplanıp paylaşılabilir.
- **Shared UI kütüphanesi:** Buton, kart, tablo, modal gibi ortak bileşenlerin tek yerde toplanması; ileride ayrı bir npm paketi veya monore workspace’i olarak taşınabilir.
- **API katmanı sadeleştirme:** HTTP çağrılarını tek tip (örn. `ApiService.get<T>`, `post<T>`) sarmalayıp hata ve token’ı merkezde yönetmek; servisler sadece endpoint ve DTO ile çalışır.

### 14.7 Yeni özellik fikirleri

- **Bildirimler:** Web Push veya uygulama içi bildirim; yeni davet, toplantı iptali, yaklaşan toplantı hatırlatması.
- **Takvim görünümü:** Toplantıları haftalık/aylık takvim üzerinde gösterme; sürükle-bırak ile tarih değiştirme.
- **Filtreleme ve export:** Listede tarih aralığı, durum, oluşturan vb. filtreler; sonuçları CSV/Excel’e aktarma.
- **Dark mode:** Tema tercihinin (localStorage + CSS değişkenleri veya Bootstrap dark) saklanması ve uygulanması.
- **Toplantı şablonları:** Sık kullanılan toplantı tipleri için şablon kaydetme ve “şablondan oluştur”.

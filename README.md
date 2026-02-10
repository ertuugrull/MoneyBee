# MoneyBee - Modern Money Transfer System

MoneyBee is a proof-of-concept money transfer system
lerini yönetmek için tasarlanmış bir mikroservis mimarisi.

## Mimari Genel Bakış

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Auth Service  │     │Customer Service │     │Transfer Service │
│     (5001)      │     │     (5002)      │     │     (5003)      │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │                       ▼                       ▼
         │              ┌─────────────────┐     ┌─────────────────┐
         │              │   KYC Service   │     │  Fraud Service  │
         │              │     (5011)      │     │     (5010)      │
         │              └─────────────────┘     └─────────────────┘
         │                                              │
         │                                      ┌───────┴───────┐
         │                                      │Exchange Rate  │
         │                                      │   Service     │
         │                                      │    (5012)     │
         │                                      └───────────────┘
         │
   API Key Auth
```

## Servisler

| Servis | Port | Açıklama |
|--------|------|----------|
| Auth Service | 5001 | API Key authentication ve rate limiting |
| Customer Service | 5002 | Müşteri yönetimi ve KYC doğrulaması |
| Transfer Service | 5003 | Para transferi işlemleri |
| Fraud Service | 5010 | Risk değerlendirmesi (harici) |
| KYC Service | 5011 | Kimlik doğrulama (harici) |
| Exchange Rate Service | 5012 | Döviz kurları (harici) |

## Kurulum

### Gereksinimler

- .NET 8.0 SDK veya üzeri
- Docker ve Docker Compose

### Yerel Çalıştırma

```bash
# Projeyi klonlayın
git clone <repository-url>
cd MoneyBee

# Solution'ı build edin
dotnet build

# Her servisi ayrı terminalde çalıştırın
cd src/MoneyBee.Auth/MoneyBee.Auth && dotnet run
cd src/MoneyBee.Customer/MoneyBee.Customer && dotnet run
cd src/MoneyBee.Transfer/MoneyBee.Transfer && dotnet run
```

### Docker ile Çalıştırma

```bash
docker-compose up -d
```

## API Kullanımı

### Varsayılan API Key

```
X-Api-Key: moneybee-default-api-key-2026
```

### Auth Service (Port 5001)

```bash
# Yeni API Key oluştur
POST /api/keys
{
    "name": "My API Key",
    "expiresInDays": 30
}

# API Key'leri listele
GET /api/keys

# API Key sil
DELETE /api/keys/{id}
```

### Customer Service (Port 5002)

```bash
# Müşteri oluştur (KYC doğrulaması ile)
POST /api/customers
{
    "name": "Ahmet",
    "surname": "Yılmaz",
    "nationalId": "12345678901",
    "phoneNumber": "+905551234567",
    "birthDate": "1990-01-15",
    "type": 0,
    "taxNumber": null
}

# Müşteri sorgula
GET /api/customers/{id}
GET /api/customers/by-national-id/{nationalId}

# Müşteri durumu güncelle
PUT /api/customers/{id}/status
{
    "status": 2
}
```

**CustomerType**: 0 = Individual, 1 = Corporate
**CustomerStatus**: 0 = Active, 1 = Passive, 2 = Blocked

### Transfer Service (Port 5003)

```bash
# Transfer oluştur
POST /api/transfers
{
    "senderCustomerId": "guid",
    "receiverCustomerId": "guid",
    "amount": 500.00,
    "currency": "TRY"
}

# Transfer sorgula
GET /api/transfers/{id}
GET /api/transfers/by-code/{transactionCode}

# Parayı teslim et
POST /api/transfers/{id}/complete

# Transfer iptal et
POST /api/transfers/{id}/cancel

# Günlük toplam transfer
GET /api/transfers/customer/{customerId}/daily-total
```

## İş Kuralları

### Transfer Kuralları
- Her transfer için fraud kontrolü yapılır
- HIGH risk → Otomatik red
- LOW risk → Onay
- 1.000 TRY üzeri → 5 dakika bekleme süresi
- Günlük limit: 10.000 TRY

### Müşteri Kuralları
- KYC doğrulaması zorunlu
- 18 yaş altı kabul edilmez
- TC Kimlik numarası algoritma doğrulaması yapılır
- Kurumsal müşteriler için vergi numarası zorunlu

## API Dokümantasyonu

Her servis için Swagger UI kullanılabilir:

- Auth: http://localhost:5001/swagger
- Customer: http://localhost:5002/swagger
- Transfer: http://localhost:5003/swagger

## Teknolojiler

- **.NET 8.0** - Web API framework
- **In-Memory Storage** - ConcurrentDictionary ile veri saklama
- **Polly** - Resilience ve retry politikaları
- **Swashbuckle** - Swagger/OpenAPI dokümantasyonu
- **Docker** - Konteynerizasyon

## Proje Yapısı

```
MoneyBee/
├── docker-compose.yml
├── MoneyBee.sln
├── src/
│   ├── MoneyBee.Shared/           # Paylaşılan kütüphane
│   │   └── MoneyBee.Shared/
│   │       ├── Exceptions/
│   │       ├── Models/
│   │       └── Utilities/
│   ├── MoneyBee.Auth/             # Auth Service
│   │   └── MoneyBee.Auth/
│   │       ├── Controllers/
│   │       ├── Data/
│   │       ├── Middleware/
│   │       ├── Models/
│   │       └── Services/
│   ├── MoneyBee.Customer/         # Customer Service
│   │   └── MoneyBee.Customer/
│   │       ├── Controllers/
│   │       ├── Data/
│   │       ├── Entities/
│   │       ├── Models/
│   │       └── Services/
│   └── MoneyBee.Transfer/         # Transfer Service
│       └── MoneyBee.Transfer/
│           ├── BackgroundServices/
│           ├── Controllers/
│           ├── Data/
│           ├── Entities/
│           ├── Models/
│           └── Services/
└── docs/
    └── MoneyBee.postman_collection.json
```

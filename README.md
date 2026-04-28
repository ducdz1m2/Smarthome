# Smarthome - Hệ Thống Quản Lý Bán Hàng & Lắp Đặt Thiết Bị Nhà Thông Minh

> **Mã nguồn demo luận văn** - Hệ thống thương mại điện tử tích hợp quản lý kho, lắp đặt, bảo hành và chat real-time cho thiết bị nhà thông minh.

---

## 📋 Mục Lục

1. [Giới thiệu](#giới-thiệu)
2. [Kiến trúc hệ thống](#kiến-trúc-hệ-thống)
3. [Công nghệ sử dụng](#công-nghệ-sử-dụng)
4. [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
5. [Cấu trúc thư mục](#cấu-trúc-thư-mục)
6. [Hướng dẫn cài đặt](#hướng-dẫn-cài-đặt)
7. [Cấu hình](#cấu-hình)
8. [Chạy dự án](#chạy-dự-án)
9. [Tài khoản mẫu](#tài-khoản-mẫu)
10. [Tính năng chính](#tính-năng-chính)
11. [Link mã nguồn](#link-mã-nguồn)

---

## 🏠 Giới thiệu

**Smarthome** là hệ thống quản lý bán hàng thiết bị nhà thông minh với các tính năng:

- **Bán hàng online**: Quản lý sản phẩm, đơn hàng, giỏ hàng, thanh toán VNPay
- **Quản lý kho**: Nhập/xuất kho, chuyển kho, đặt trước sản phẩm
- **Lắp đặt thông minh**: Đặt lịch lắp đặt, phân công kỹ thuật viên theo khu vực
- **Bảo hành**: Quản lý yêu cầu bảo hành, lịch sử bảo hành
- **Chat real-time**: Trao đổi giữa khách hàng và kỹ thuật viên qua SignalR
- **Thông báo**: Push notification, email, real-time notifications
- **AI Speech**: Nhận diện giọng nói tìm kiếm sản phẩm (Python FastAPI)

---

## 🏗️ Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────────┐
│                         Web (Blazor Server)                  │
│                     - UI với MudBlazor                       │
│                     - SignalR Hubs (Chat, Notifications)       │
│                     - JWT Authentication                     │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────┐
│                      Application Layer                       │
│                     - Services, DTOs                         │
│                     - AutoMapper                             │
│                     - Domain Event Handlers                  │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────┐
│                        Domain Layer                          │
│                     - Entities, Value Objects                │
│                     - Domain Events                          │
│                     - Repository Interfaces                  │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────┐
│                      Infrastructure Layer                    │
│                     - Entity Framework Core                   │
│                     - SQL Server                             │
│                     - Repositories                           │
│                     - Identity                               │
└─────────────────────────────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────┐
│                    Speech Service (Python)                     │
│                     - FastAPI + Whisper                      │
│                     - Port: 8003                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🛠️ Công nghệ sử dụng

| Layer | Công nghệ | Phiên bản |
|-------|-----------|-----------|
| **Backend** | .NET 10 / ASP.NET Core | 10.0.6 |
| **UI** | Blazor Server + MudBlazor | 9.2.0 |
| **Database** | SQL Server + EF Core | 9.0.0 |
| **Real-time** | SignalR | 10.0.6 |
| **Authentication** | ASP.NET Identity + JWT | - |
| **Payment** | VNPay Sandbox | - |
| **PDF** | QuestPDF | 2025.1.0 |
| **Email** | FluentEmail + Gmail SMTP | - |
| **Push Notifications** | Web Push | 1.0.12 |
| **Speech AI** | Python + FastAPI + Whisper | - |
| **Mapping** | AutoMapper | 14.0.0 |

---

## 💻 Yêu cầu hệ thống

### Phần mềm bắt buộc:

1. **.NET 10 SDK** - [Tải tại đây](https://dotnet.microsoft.com/download/dotnet/10.0)
2. **SQL Server** (Express hoặc Developer Edition)
3. **Python 3.10+** (cho Speech Service)
4. **Visual Studio 2022** hoặc **VS Code** (khuyến nghị VS 2022)
5. **Git** (để clone repository)

### Phần cứng tối thiểu:

- **RAM**: 8GB (khuyến nghị 16GB)
- **Disk**: 10GB trống (chứa models AI ~2GB)
- **CPU**: 4 cores
- **OS**: Windows 10/11, Linux, hoặc macOS

---

## 📁 Cấu trúc thư mục

```
Smarthome/
├── Web/                          # Blazor Server UI
│   ├── Components/               # Razor Components
│   ├── Pages/                    # Trang chính
│   ├── Hubs/                     # SignalR Hubs
│   ├── Services/                 # Web-specific services
│   ├── appsettings.json          # Cấu hình chính
│   └── Web.csproj
│
├── Application/                  # Application Layer
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Interfaces/               # Service interfaces
│   ├── Services/                 # Business logic
│   ├── Mappings/                 # AutoMapper profiles
│   └── Application.csproj
│
├── Domain/                       # Domain Layer
│   ├── Entities/                 # Domain entities
│   ├── Events/                   # Domain events
│   ├── ValueObjects/             # Value objects
│   └── Domain.csproj
│
├── Infrastructure/               # Infrastructure Layer
│   ├── Data/                     # DbContext, Configurations
│   ├── Repositories/             # Repository implementations
│   └── Infrastructure.csproj
│
├── speech-service/               # Python Speech Service
│   ├── main.py                   # FastAPI server
│   ├── requirements.txt          # Python dependencies
│   └── models/                   # Whisper models (~2GB)
│
├── docs/                         # Documentation
│   ├── database-schema.md        # Schema chi tiết
│   └── entities-class-diagram.md # Class diagram
│
├── Smarthome.slnx                # Solution file
└── README.md                     # File này
```

---

## 🔧 Hướng dẫn cài đặt

### Bước 1: Clone hoặc giải nén mã nguồn

```bash
# Nếu có Git
git clone <repository-url> Smarthome
cd Smarthome

# Hoặc giải nén file zip đã đóng gói
tar -xzf Smarthome-source.tar.gz
# hoặc
tar -xzf Smarthome-source.zip
cd Smarthome
```

### Bước 2: Cài đặt SQL Server

1. Tải **SQL Server Express** từ: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. Cài đặt với instance name: `SQLEXPRESS`
3. Bật **SQL Server Authentication** (mixed mode)
4. Tạo database tên: `Smarthome`

### Bước 3: Cấu hình Connection String

Mở file `Web/appsettings.json`, sửa connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=Smarthome;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Nếu dùng SQL Authentication:
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=Smarthome;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Bước 4: Restore NuGet Packages

```bash
dotnet restore Smarthome.slnx
```

Hoặc trong Visual Studio:
- Mở `Smarthome.slnx`
- Chờ Visual Studio restore packages tự động

### Bước 5: Chạy Migration (Tạo database schema)

```bash
cd Infrastructure
dotnet ef database update --startup-project ../Web
```

Hoặc nếu chưa có EF Core CLI:
```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project Infrastructure --startup-project Web
```

### Bước 6: Cài đặt Python Speech Service

```bash
cd speech-service

# Tạo virtual environment (khuyến nghị)
python -m venv venv

# Windows:
venv\Scripts\activate

# Linux/Mac:
source venv/bin/activate

# Cài đặt dependencies
pip install -r requirements.txt

# Download Whisper models (tự động khi chạy lần đầu)
python download_model.py
```

**Lưu ý**: Models AI (~2GB) sẽ tự động download khi chạy service lần đầu.

---

## ⚙️ Cấu hình

### 1. JWT Settings (Đã cấu hình sẵn trong appsettings.json)

```json
"JwtSettings": {
  "SecretKey": "SmarthomeSecretKey_ChangeThis_InProduction_2026!",
  "Issuer": "SmarthomeAPI",
  "Audience": "SmarthomeClient",
  "ExpiresInDays": 7
}
```

> ⚠️ **Quan trọng**: Đổi `SecretKey` khi deploy production!

### 2. VNPay Sandbox (Thanh toán)

Đã cấu hình sẵn với tài khoản test:
```json
"VNPay": {
  "TmnCode": "AZEGNZRE",
  "HashSecret": "F7V7T4T91GKE2SCU4D42OJ1B952RPBPG",
  "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
}
```

**Test thẻ VNPay Sandbox**:
- Ngân hàng: NCB
- Số thẻ: `9704198526191432198`
- Tên: `NGUYEN VAN A`
- Ngày hết hạn: `07/15`
- OTP: `123456`

### 3. Email SMTP (Gmail)

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "User": "lengocduc051@gmail.com",
  "Password": "jmdc grtn cifj uetg",
  "FromName": "Smart-home",
  "FromEmail": "lengocduc051@gmail.com"
}
```

### 4. Speech Service

```json
"SpeechService": {
  "Url": "http://localhost:8003"
}
```

---

## 🚀 Chạy dự án

### Cách 1: Chạy từ Visual Studio (Khuyến nghị)

1. Mở `Smarthome.slnx` trong Visual Studio 2022
2. Set startup project: **Web**
3. Chọn IIS Express hoặc Kestrel
4. Nhấn **F5** hoặc **Ctrl+F5**

### Cách 2: Chạy từ Command Line

**Terminal 1 - Chạy Web App:**
```bash
cd Web
dotnet run
# Hoặc với HTTPS
dotnet run --urls "https://localhost:7298;http://localhost:5298"
```

**Terminal 2 - Chạy Speech Service (Python):**
```bash
cd speech-service

# Windows:
venv\Scripts\activate
python main.py

# Service sẽ chạy tại: http://localhost:8003
```

### Cách 3: Chạy song song (Script)

Tạo file `start-all.bat` (Windows):
```batch
@echo off
start "Speech Service" cmd /k "cd speech-service && venv\Scripts\activate && python main.py"
start "Web App" cmd /k "cd Web && dotnet run"
echo Đang khởi động Smarthome...
echo Web: https://localhost:7298
echo Speech API: http://localhost:8003
```

---

## 👤 Tài khoản mẫu

Sau khi chạy, hệ thống tự động tạo các tài khoản sau:

| Vai trò | Email | Mật khẩu |
|---------|-------|----------|
| **Admin** | `admin@smarthome.com` | `Admin123!` |
| **Customer** | `customer@example.com` | `Customer123!` |
| **Technician** | `tech@example.com` | `Tech123!` |

Hoặc tạo tài khoản mới tại trang `/register`

---

## ⭐ Tính năng chính

### 1. Quản lý Catalog
- Quản lý sản phẩm, danh mục, thương hiệu
- Biến thể sản phẩm (màu sắc, kích thước)
- Upload hình ảnh
- Khuyến mãi & Mã giảm giá

### 2. Quản lý Đơn hàng
- Giỏ hàng, đặt hàng
- Thanh toán VNPay
- Phân bổ kho tự động
- Xuất hóa đơn PDF

### 3. Quản lý Kho
- Nhập/xuất kho
- Chuyển kho giữa các kho
- Đặt trước sản phẩm
- Theo dõi tồn kho real-time

### 4. Lắp đặt & Bảo hành
- Đặt lịch lắp đặt theo khu vực
- Phân công kỹ thuật viên tự động
- Theo dõi trạng thái: Đã phân công → Xác nhận → Chuẩn bị → Di chuyển → Lắp đặt → Hoàn thành
- Hệ thống bảo hành, yêu cầu bảo hành

### 5. Chat & Notifications
- Chat real-time giữa Customer ↔ Technician
- Push notifications (browser)
- Email notifications
- SignalR hubs cho real-time updates

### 6. AI Speech Recognition
- Tìm kiếm sản phẩm bằng giọng nói
- Whisper model hỗ trợ tiếng Việt

---

## 📦 Link mã nguồn

### Cách 1: Upload lên GitHub

```bash
# Tạo repository trên GitHub, sau đó:
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/<username>/Smarthome.git
git push -u origin main
```

**Link repository**: `https://github.com/<username>/Smarthome`

### Cách 2: Đóng gói nén

```bash
# Windows PowerShell:
Compress-Archive -Path .\Smarthome -DestinationPath .\Smarthome-source.zip

# Hoặc dùng 7-Zip:
7z a Smarthome-source.7z Smarthome/

# Linux/Mac:
tar -czvf Smarthome-source.tar.gz Smarthome/
```

### Cách 3: Dataset lớn (Models AI)

Models Whisper (~2GB) sẽ tự động download khi chạy speech service lần đầu.

Nếu cần backup models:
```bash
cd speech-service/models
tar -czvf whisper-models.tar.gz faster-whisper/
```

Upload lên Google Drive hoặc Dropbox và tạo file `MODELS_LINK.txt`:
```
Download Whisper Models:
https://drive.google.com/file/d/<file-id>/view?usp=sharing

Hoặc tự động download khi chạy: python download_model.py
```

---

## 🧪 Test Luồng Chính

Xem chi tiết kịch bản test tại: [TEST_INSTALLATION_FLOW.md](./TEST_INSTALLATION_FLOW.md)

**Luồng test nhanh**:
1. Đăng nhập Admin → Tạo sản phẩm cần lắp đặt
2. Đăng nhập Customer → Đặt hàng → Thanh toán VNPay
3. Admin xác nhận đơn → Tạo lịch lắp đặt → Chọn kỹ thuật viên
4. Đăng nhập Technician → Tiếp nhận lịch → Chuẩn bị vật tư → Hoàn thành
5. Customer chat với Technician trong lịch lắp đặt

---

## 📚 Tài liệu tham khảo

- [Database Schema](./docs/database-schema.md)
- [Class Diagram](./docs/entities-class-diagram.md)
- [Test Installation Flow](./TEST_INSTALLATION_FLOW.md)

---

## 🐛 Troubleshooting

| Lỗi | Giải pháp |
|-----|-----------|
| `Cannot connect to SQL Server` | Kiểm tra SQL Server đã chạy, connection string đúng |
| `dotnet ef không tìm thấy` | Chạy: `dotnet tool install --global dotnet-ef` |
| `Speech service không chạy` | Kiểm tra port 8003 có bị chiếm, Python đã activate venv |
| `Whisper model download fail` | Chạy `python download_model.py` trước khi chạy main.py |
| `Port 7298 bị chiếm` | Chạy: `dotnet run --urls "https://localhost:7299"` |

---

## 📧 Liên hệ

Nếu gặp vấn đề khi chạy demo, vui lòng liên hệ:
- Email: `lengocduc051@gmail.com`
- GitHub Issues: [tạo issue tại repository]

---

## 📄 License

Mã nguồn này được cung cấp cho mục đích **demo luận văn**. Không sử dụng cho mục đích thương mại mà không có sự cho phép.

---

**Phiên bản**: 1.0  
**Ngày cập nhật**: Tháng 4/2026  
**Tác giả**: Luận văn tốt nghiệp

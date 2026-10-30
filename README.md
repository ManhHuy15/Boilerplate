# Fullstack Boilerplate

Dự án mẫu chuẩn (Boilerplate) kết hợp **ASP.NET Core Web API (.NET 10)** và **Next.js App Router (React 19)** với luồng xác thực bảo mật (JWT + HttpOnly Cookie Refresh Token) và cấu trúc sẵn sàng mở rộng.

## Cấu trúc dự án

- [`BE/`](./BE): Backend Web API .NET 10 theo mô hình Clean Architecture (Domain, Application, Infrastructure, API) với EF Core Code First & PostgreSQL.
- [`FE/`](./FE): Frontend Next.js 16 App Router, TypeScript strict, Tailwind CSS v4, HeroUI component library và Zustand.
- [`.agents/rules/`](./.agents/rules): Quy chuẩn kiến trúc kỹ thuật chi tiết dành cho Backend (`BE.md`) và Frontend (`FE.md`).

## Bắt đầu dự án mới từ Boilerplate

Sau khi clone boilerplate về máy, bạn có thể đổi tên dự án thành tên nghiệp vụ của bạn và khởi tạo Git mới bằng script tự động:

**Trên Linux / macOS (Bash):**
```bash
./rename-project.sh MyStore --reset-git
```

**Trên Windows (PowerShell):**
```powershell
.\rename-project.ps1 -NewName MyStore -ResetGit
```

Script sẽ tự động:
1. Đổi tên toàn bộ namespace C#, projects `.csproj`, solution `.slnx`, thư mục backend.
2. Đổi tên package Frontend, title, brand name, cookies, database name.
3. Cập nhật `docker-compose.yml` và cấu hình môi trường.
4. Xóa lịch sử Git cũ và tạo commit khởi đầu (`feat: initialize MyStore project from boilerplate`) sẵn sàng để bạn push lên repo mới.

---

## Khởi chạy dự án

### 1. Backend (`BE/`)
Chi tiết xem thêm tại [`BE/README.md`](./BE/README.md).

```powershell
# Cấu hình biến môi trường
Copy-Item BE/Boilerplate.API/.env.example BE/Boilerplate.API/.env

# Build & Run API (mặc định tại http://localhost:8080)
dotnet build BE/Boilerplate.slnx
dotnet run --project BE/Boilerplate.API
```

### 2. Frontend (`FE/`)
Chi tiết xem thêm tại [`FE/README.md`](./FE/README.md).

```bash
# Cấu hình biến môi trường (nếu cần)
copy FE\.env.example FE\.env.local

# Cài đặt package và chạy dev server (mặc định tại http://localhost:3000)
cd FE
npm install
npm run dev
```

### 3. Khởi chạy toàn bộ hệ thống bằng Docker Compose (Khuyên dùng cho Production)

Chỉ cần một lệnh duy nhất để khởi động toàn bộ PostgreSQL, Backend API và Frontend:

```bash
docker compose up --build -d
```

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8080
- **Health Check**: http://localhost:8080/api/health
- **PostgreSQL**: localhost:5432

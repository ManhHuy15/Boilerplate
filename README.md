# Fullstack Boilerplate

Dự án mẫu chuẩn (Boilerplate) kết hợp **ASP.NET Core Web API (.NET 10)** và **Next.js App Router (React 19)** với luồng xác thực bảo mật (JWT + HttpOnly Cookie Refresh Token) và cấu trúc sẵn sàng mở rộng.

## Cấu trúc dự án

- [`BE/`](./BE): Backend Web API .NET 10 theo mô hình Clean Architecture (Domain, Application, Infrastructure, API) với EF Core Code First & PostgreSQL.
- [`FE/`](./FE): Frontend Next.js 16 App Router, TypeScript strict, Tailwind CSS v4, HeroUI component library và Zustand.
- [`.agents/rules/`](./.agents/rules): Quy chuẩn kiến trúc kỹ thuật chi tiết dành cho Backend (`BE.md`) và Frontend (`FE.md`).

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
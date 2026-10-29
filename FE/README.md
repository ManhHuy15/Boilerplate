# Boilerplate Frontend

Frontend dùng Next.js App Router, React, TypeScript strict và Tailwind CSS. Auth được kết nối với Backend .NET chạy mặc định tại cổng `8080`.

## Chạy local

```bash
copy .env.example .env.local
npm install
npm run dev
```

Mở `http://localhost:3000`. Next.js sẽ proxy `/api/auth/*` sang Backend nên trình duyệt không cần gọi chéo origin và refresh token HttpOnly vẫn hoạt động đúng.

## Kiểm tra

```bash
npm run lint
npm run typecheck
npm run build
```

Biến môi trường `API_BASE_URL` chỉ được dùng phía server của Next.js, không đưa bí mật vào biến có tiền tố `NEXT_PUBLIC_`.

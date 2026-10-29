import type { Metadata } from "next";
import type { ReactNode } from "react";
import { AppToaster } from "@/components/shared/AppToaster";
import { AuthSessionInitializer } from "@/features/auth/components/AuthSessionInitializer";
import "./globals.css";

export const metadata: Metadata = {
  title: {
    default: "Boilerplate — Nền tảng để bắt đầu nhanh",
    template: "%s | Boilerplate",
  },
  description: "Frontend boilerplate bảo mật, rõ ràng và sẵn sàng mở rộng.",
};

export default function RootLayout({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <html lang="vi">
      <body className="antialiased">
        <AuthSessionInitializer>{children}</AuthSessionInitializer>
        <AppToaster />
      </body>
    </html>
  );
}

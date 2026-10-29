import type { Metadata } from "next";
import { AuthForm } from "@/features/auth/components/AuthForm";
import { AUTH_FORM_MODES } from "@/features/auth/constants/auth";

export const loginPageMetadata: Metadata = { title: "Đăng nhập" };

export function LoginPage() {
  return (
    <div className="w-full max-w-[430px]">
      <p className="mb-3 text-[.68rem] font-extrabold tracking-[.14em] text-brand-leaf uppercase">Chào mừng trở lại</p>
      <h1 className="m-0 font-serif text-[clamp(2.4rem,4vw,3.25rem)] font-normal tracking-[-.05em]">Đăng nhập tài khoản</h1>
      <p className="mt-3 mb-[34px] text-sm text-[#697269]">Tiếp tục nơi bạn đã dừng lại.</p>
      <AuthForm mode={AUTH_FORM_MODES.login} />
    </div>
  );
}

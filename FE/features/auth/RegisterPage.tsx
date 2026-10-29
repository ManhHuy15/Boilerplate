import type { Metadata } from "next";
import { AuthForm } from "@/features/auth/components/AuthForm";
import { AUTH_FORM_MODES } from "@/features/auth/constants/auth";

export const registerPageMetadata: Metadata = { title: "Đăng ký" };

export function RegisterPage() {
  return (
    <div className="w-full max-w-[430px]">
      <p className="mb-3 text-[.68rem] font-extrabold tracking-[.14em] text-brand-leaf uppercase">Bắt đầu trong một phút</p>
      <h1 className="m-0 font-serif text-[clamp(2.4rem,4vw,3.25rem)] font-normal tracking-[-.05em]">Tạo tài khoản mới</h1>
      <p className="mt-3 mb-[34px] text-sm text-[#697269]">Không cần email hay bước xác nhận phức tạp.</p>
      <AuthForm mode={AUTH_FORM_MODES.register} />
    </div>
  );
}

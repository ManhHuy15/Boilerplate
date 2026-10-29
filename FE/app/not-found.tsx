import Link from "next/link";
import { Brand } from "@/components/shared/Brand";
import { APP_ROUTES } from "@/shared/constants/routes";

export default function NotFound() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center px-[30px] text-center">
      <Brand />
      <span className="mt-14 font-serif text-7xl leading-none text-brand-leaf">404</span>
      <h1 className="mt-[18px] mb-2 font-serif text-[2.5rem] font-normal tracking-[-.05em]">Trang này không tồn tại.</h1>
      <p className="mb-7 text-[#697269]">Đường dẫn có thể đã thay đổi hoặc chưa được xây dựng.</p>
      <Link href={APP_ROUTES.home} className="button button--primary min-h-12 bg-brand-ink px-[22px] text-white hover:bg-brand-deep">
        Về trang chủ
      </Link>
    </main>
  );
}

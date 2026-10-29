import Link from "next/link";
import type { ReactNode } from "react";
import { Brand } from "@/components/shared/Brand";
import { APP_ROUTES } from "@/shared/constants/routes";

export default function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <main className="grid min-h-screen bg-white md:grid-cols-[minmax(500px,.9fr)_1.1fr]">
      <section className="flex min-h-screen flex-col px-[22px] py-5 sm:px-[34px] md:px-[clamp(34px,6vw,90px)] md:py-8">
        <header><Brand /></header>
        <div className="grid flex-1 place-items-center py-12 md:py-14">{children}</div>
        <footer className="flex justify-between text-[.7rem] text-[#697269]">
          <span>© {new Date().getFullYear()} Boilerplate</span>
          <Link href={APP_ROUTES.home} className="no-underline hover:underline">Về trang chủ</Link>
        </footer>
      </section>

      <aside aria-hidden="true" className="relative m-3 hidden overflow-hidden rounded-xl bg-brand-deep text-white md:block">
        <div className="absolute -top-[180px] -right-40 size-[560px] rounded-full border-[110px] border-[rgba(185,244,74,.08)]" />
        <div className="absolute inset-0 opacity-10 [mask-image:linear-gradient(to_bottom,#000,transparent_75%)] [background-image:linear-gradient(rgba(255,255,255,.5)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,.5)_1px,transparent_1px)] [background-size:48px_48px]" />
        <div className="absolute right-[clamp(45px,7vw,110px)] bottom-[clamp(60px,8vw,105px)] left-[clamp(45px,6vw,90px)] z-2">
          <span className="block h-[60px] font-serif text-8xl leading-none text-brand-lime">“</span>
          <blockquote className="my-[12px] mb-[35px] max-w-[650px] font-serif text-[clamp(2rem,3.1vw,3.5rem)] leading-[1.12] tracking-[-.04em]">
            Nền móng tốt giúp đội ngũ dành thời gian cho điều làm sản phẩm khác biệt.
          </blockquote>
          <div className="flex items-center gap-3">
            <span className="h-0.5 w-[34px] bg-brand-lime" />
            <div className="flex flex-col text-xs"><strong>Build once</strong><span className="text-[#9db2a6]">Scale with confidence</span></div>
          </div>
        </div>
        <div className="absolute top-[70px] left-[75px] size-[220px] rounded-full border border-[rgba(185,244,74,.25)]" />
        <div className="absolute top-[140px] left-[145px] size-20 rounded-full bg-brand-lime shadow-[0_0_70px_rgba(185,244,74,.24)]" />
      </aside>
    </main>
  );
}

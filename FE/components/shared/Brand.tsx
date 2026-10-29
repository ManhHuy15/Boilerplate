import Link from "next/link";
import { APP_ROUTES } from "@/shared/constants/routes";

export function Brand({ compact = false }: { compact?: boolean }) {
  return (
    <Link
      href={APP_ROUTES.home}
      aria-label="Boilerplate - Trang chủ"
      className="inline-flex items-center gap-2.5 no-underline"
    >
      <span
        aria-hidden="true"
        className="grid w-[18px] -rotate-[8deg] grid-cols-[repeat(2,8px)] grid-rows-[repeat(2,8px)] gap-0.5"
      >
        <span className="row-span-2 block rounded-[2px] bg-current" />
        <span className="block rounded-[2px] bg-current" />
        <span className="block rounded-[2px] bg-brand-leaf" />
      </span>
      {!compact && <span className="font-extrabold tracking-[-.04em]">Boilerplate</span>}
    </Link>
  );
}

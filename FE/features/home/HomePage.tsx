import Link from "next/link";
import { Card } from "@heroui/react";
import { ArrowRight, Check } from "lucide-react";
import { Brand } from "@/components/shared/Brand";
import { APP_ROUTES } from "@/shared/constants/routes";

const shellClass = "mx-auto w-full max-w-[1180px] px-[14px] sm:px-4 md:px-6";

const features = [
  { number: "01", title: "Auth đã sẵn sàng", description: "Đăng ký, đăng nhập, làm mới phiên và đăng xuất theo một luồng thống nhất." },
  { number: "02", title: "Type-safe từ đầu", description: "TypeScript strict và kiểm tra dữ liệu API tại runtime, không ép kiểu thiếu an toàn." },
  { number: "03", title: "Dễ mang sang dự án mới", description: "Cấu trúc theo feature giúp thay thương hiệu và bổ sung nghiệp vụ mà không phải làm lại nền móng." },
] as const;

export function HomePage() {
  return (
    <main className="overflow-hidden bg-brand-canvas">
      <header className={`${shellClass} flex h-[72px] items-center justify-between sm:h-[86px]`}>
        <Brand />
        <nav aria-label="Điều hướng chính" className="hidden items-center gap-[34px] text-sm text-[#59625a] md:flex">
          <a className="no-underline hover:text-brand-leaf" href="#foundation">Nền tảng</a>
          <a className="no-underline hover:text-brand-leaf" href="#architecture">Kiến trúc</a>
        </nav>
        <div className="flex items-center gap-[22px]">
          <Link href={APP_ROUTES.login} className="hidden text-sm font-bold no-underline sm:block">Đăng nhập</Link>
          <Link href={APP_ROUTES.register} className="button button--primary min-h-[39px] bg-brand-ink px-[18px] text-white hover:bg-brand-deep">Bắt đầu</Link>
        </div>
      </header>

      <section className={`${shellClass} grid items-center gap-16 pt-11 pb-[76px] sm:pt-[62px] md:min-h-[670px] md:grid-cols-[.92fr_1.08fr] md:gap-[52px] md:pt-[54px] md:pb-20`}>
        <div className="sm:max-w-[800px]">
          <div className="mb-5 flex items-center gap-2.5 text-[.7rem] font-extrabold tracking-[.16em] text-[#526057] uppercase">
            <span className="h-0.5 w-7 bg-brand-leaf" />Next.js + .NET 10
          </div>
          <h1 className="m-0 max-w-[660px] font-serif text-[clamp(3.1rem,6.2vw,6.2rem)] leading-[.93] font-normal tracking-[-.07em]">
            Xây sản phẩm.<br /><em className="font-normal text-brand-leaf">Không xây lại nền móng.</em>
          </h1>
          <p className="mt-[30px] max-w-[550px] text-[.94rem] leading-7 text-[#697269] sm:text-[1.05rem]">
            Một điểm khởi đầu gọn gàng cho sản phẩm tiếp theo của bạn — auth an toàn, kiến trúc rõ ràng và trải nghiệm đã được chăm chút.
          </p>
          <div className="mt-[34px] flex flex-col gap-3 sm:flex-row">
            <Link href={APP_ROUTES.register} className="button button--primary min-h-12 bg-brand-lime px-[22px] text-brand-ink hover:bg-[#c7fa66]">
              Tạo tài khoản <ArrowRight size={17} />
            </Link>
            <a href="#foundation" className="button button--outline min-h-12 border-[#cfd4c9] px-[22px] text-brand-ink hover:bg-white/60">Khám phá nền tảng</a>
          </div>
          <div className="mt-8 flex flex-wrap gap-3 text-xs text-[#657067] sm:gap-5">
            {["HttpOnly refresh token", "TypeScript strict", "Responsive"].map((item) => (
              <span key={item} className="flex items-center gap-1"><Check size={17} className="text-brand-leaf" />{item}</span>
            ))}
          </div>
        </div>

        <div className="relative flex min-h-[300px] items-start sm:min-h-[430px] sm:items-center md:min-h-[510px] md:min-w-[660px]">
          <div className="absolute right-0 -z-1 size-[340px] rounded-full bg-[#e8eadf] md:size-[550px]" />
          <div className="absolute right-[-48px] -z-1 size-[650px] rounded-full border border-[#d3d8cc]" />
          <div className="absolute right-[50px] -z-1 size-[455px] animate-[spin_35s_linear_infinite] rounded-full border border-dashed border-[#d3d8cc] motion-reduce:animate-none" />
          <div className="w-full overflow-hidden rounded-[14px] border border-[#d4d9ce] bg-white shadow-[0_24px_70px_rgba(20,31,24,.12)] sm:[transform:perspective(1200px)_rotateY(-4deg)_rotateX(1deg)]">
            <div className="flex h-[52px] items-center gap-3.5 border-b border-[#e8eae4] px-3.5">
              <Brand compact /><span className="ml-auto h-2.5 w-[120px] rounded-full bg-[#eef0e9]" /><span className="grid size-[27px] place-items-center rounded-full bg-brand-deep text-[.55rem] font-extrabold text-white">BA</span>
            </div>
            <div className="grid min-h-[250px] grid-cols-[35px_1fr] md:min-h-[370px] sm:grid-cols-[54px_1fr]">
              <aside className="flex flex-col items-center gap-2.5 border-r border-[#eceee8] bg-[#fafbf7] pt-[15px] sm:gap-[15px] sm:pt-[22px]">
                {[0, 1, 2, 3].map((item) => <span key={item} className={`size-3 rounded-[5px] sm:size-[18px] ${item === 0 ? "bg-brand-lime shadow-[0_0_0_5px_#f0fad9]" : "bg-[#e2e5dd]"}`} />)}
              </aside>
              <div className="p-3.5 sm:p-[27px]">
                <div className="mb-[22px] flex items-center justify-between"><span className="h-[15px] w-[135px] rounded bg-[#263029]" /><span className="h-[27px] w-[70px] rounded-md bg-brand-lime" /></div>
                <div className="grid grid-cols-3 gap-[11px]">
                  {[["Active users", "2,408", "+12.4%"], ["Conversion", "18.6%", "+4.2%"], ["Sessions", "8,921", "+9.8%"]].map(([label, value, growth]) => (
                    <div key={label} className="relative flex flex-col gap-2 rounded-lg border border-[#e4e7df] p-2.5 sm:p-[15px]">
                      <span className="text-[.55rem] text-[#859087]">{label}</span><strong className="font-serif text-lg">{value}</strong><b className="absolute right-3 bottom-[15px] hidden text-[.52rem] text-[#5c9013] sm:block">{growth}</b>
                    </div>
                  ))}
                </div>
                <div className="mt-3 rounded-lg border border-[#e4e7df] p-2.5 pb-0 sm:p-4 sm:pb-0">
                  <div className="flex justify-between text-[.6rem] font-bold"><span>Growth</span><span className="font-normal text-[#8a938c]">Last 30 days</span></div>
                  <svg viewBox="0 0 520 160" role="img" aria-label="Biểu đồ tăng trưởng minh họa" className="mt-2.5 block w-full">
                    <defs><linearGradient id="chart-fill" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#b9f44a" stopOpacity=".45" /><stop offset="1" stopColor="#b9f44a" stopOpacity="0" /></linearGradient></defs>
                    <path d="M0 136 C55 125 70 104 118 112 S190 75 230 86 S300 54 340 62 S410 23 450 38 S490 17 520 14 L520 160 L0 160Z" fill="url(#chart-fill)" /><path d="M0 136 C55 125 70 104 118 112 S190 75 230 86 S300 54 340 62 S410 23 450 38 S490 17 520 14" fill="none" stroke="#8fce27" strokeWidth="3" strokeLinecap="round" />
                  </svg>
                </div>
              </div>
            </div>
          </div>
          <div className="absolute right-[5px] bottom-2.5 flex items-center gap-2 rounded-lg border border-[#dce0d7] bg-[rgba(255,255,252,.94)] p-[13px_15px] text-[.7rem] shadow-[0_14px_35px_rgba(25,37,28,.15)] backdrop-blur-sm md:right-[-22px] md:bottom-[38px]">
            <span className="size-[7px] rounded-full bg-[#70bd39] shadow-[0_0_0_4px_#e8f6de]" />API proxy <b className="ml-[7px] text-[#8a918b]">ready</b>
          </div>
        </div>
      </section>

      <section id="foundation" className={`${shellClass} border-t border-[#dfe2d8] py-20 md:py-[105px]`}>
        <div className="mb-8 items-end justify-between md:mb-[70px] md:flex">
          <div className="flex items-center gap-2.5 text-[.7rem] font-extrabold tracking-[.16em] text-[#526057] uppercase"><span className="h-0.5 w-7 bg-brand-leaf" />Nền tảng dùng chung</div>
          <h2 className="mt-7 font-serif text-[clamp(2.7rem,4vw,4.4rem)] leading-[1.04] font-normal tracking-[-.055em] md:mt-0">Những phần lặp lại,<br />đã được giải quyết.</h2>
        </div>
        <div id="architecture" className="grid gap-4 border-t border-[#cfd4ca] pt-4 md:grid-cols-3">
          {features.map((feature) => (
            <Card key={feature.number} variant="transparent" className="border border-[#dfe2d8] bg-white/40">
              <Card.Content className="p-6">
                <span className="text-xs font-extrabold text-brand-leaf">{feature.number}</span>
                <h3 className="mt-7 mb-3 text-lg font-bold">{feature.title}</h3>
                <p className="m-0 text-sm leading-[1.65] text-[#697269]">{feature.description}</p>
              </Card.Content>
            </Card>
          ))}
        </div>
      </section>

      <footer className={`${shellClass} flex min-h-[150px] flex-col items-center justify-between gap-[15px] border-t border-[#dfe2d8] py-8 text-[.78rem] text-[#7a827b] sm:min-h-[110px] sm:flex-row sm:py-0`}>
        <Brand /><span>Khởi đầu chắc chắn. Phát triển tự tin.</span><span>© {new Date().getFullYear()}</span>
      </footer>
    </main>
  );
}

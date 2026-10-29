"use client";

import { Alert, Button, Card, Spinner } from "@heroui/react";
import { LayoutGrid, LogOut, ShieldCheck, UserRound } from "lucide-react";
import { Brand } from "@/components/shared/Brand";
import { useDashboard } from "@/features/dashboard/hooks/useDashboard";

const ACCOUNT_ID_DISPLAY_LENGTH = 4;
const ACCOUNT_ID_PAD_CHARACTER = "0";

const navigation = [
  { label: "Tổng quan", icon: LayoutGrid, active: true },
  { label: "Tài khoản", icon: UserRound, active: false },
  { label: "Bảo mật", icon: ShieldCheck, active: false },
] as const;

export function DashboardScreen() {
  const { account, isLoading, initials, isLoggingOut, isRefreshing, notice, handleLogout, handleRefresh } = useDashboard();

  if (isLoading || !account) {
    return (
      <main className="relative flex min-h-screen items-center justify-center gap-3 bg-brand-canvas">
        <div className="absolute top-8 left-[38px]"><Brand /></div>
        <Spinner size="md" aria-label="Đang khôi phục phiên đăng nhập" />
        <p className="text-[.83rem] text-[#697269]">Đang khôi phục phiên đăng nhập…</p>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-[#f2f3ed] md:grid md:grid-cols-[236px_1fr]">
      <aside className="flex h-[70px] w-full flex-row items-center border-b border-[#dfe2d8] bg-[#fbfcf7] px-6 md:sticky md:top-0 md:h-screen md:w-auto md:flex-col md:items-stretch md:border-r md:border-b-0 md:px-5 md:pt-[30px] md:pb-[22px]">
        <div className="md:ml-[9px]"><Brand /></div>
        <nav aria-label="Điều hướng bảng điều khiển" className="mt-[60px] hidden gap-[5px] md:grid">
          <p className="mx-2.5 mb-2.5 text-[.58rem] font-extrabold tracking-[.15em] text-[#9ba19b] uppercase">Workspace</p>
          {navigation.map((item) => {
            const Icon = item.icon;
            return (
              <Button
                key={item.label}
                variant="ghost"
                className={`h-[42px] w-full justify-start px-3 text-[.78rem] ${item.active ? "bg-[#eaf4d6] font-extrabold text-[#29421d]" : "font-medium text-[#657067] hover:bg-[#eef1e8] hover:text-brand-ink"}`}
              >
                <Icon size={18} />{item.label}
              </Button>
            );
          })}
        </nav>
        <div className="ml-auto flex flex-col gap-[7px] md:mt-auto md:ml-0">
          <div className="hidden items-center rounded-lg border border-[#e0e4da] p-3 text-[.65rem] text-[#788179] md:flex">
            <span className="mr-2 size-[7px] rounded-full bg-[#6db337]" />Backend<strong className="ml-auto text-[#588c31]">online</strong>
          </div>
          <Button
            variant="ghost"
            onPress={handleLogout}
            isDisabled={isLoggingOut}
            isPending={isLoggingOut}
            className="h-[42px] justify-start text-[.78rem] text-[#657067] hover:bg-[#eef1e8] hover:text-brand-ink"
          >
            {isLoggingOut ? <Spinner size="sm" color="current" /> : <LogOut size={18} />}
            Đăng xuất
          </Button>
        </div>
      </aside>

      <section className="min-w-0">
        <header className="flex h-[78px] items-center justify-between border-b border-[#dfe2d8] bg-[rgba(251,252,247,.82)] px-5 backdrop-blur-[10px] md:h-[92px] md:px-[clamp(28px,4vw,58px)]">
          <div><p className="mb-[5px] text-[.62rem] tracking-[.13em] text-[#969d96] uppercase">Dashboard</p><h1 className="m-0 font-serif text-[1.2rem] font-normal tracking-[-.03em] md:text-[1.48rem]">Xin chào, {account.username}</h1></div>
          <div className="flex items-center gap-2.5">
            <span className="grid size-[38px] place-items-center rounded-full bg-brand-deep text-[.69rem] font-extrabold text-white">{initials}</span>
            <div className="hidden flex-col sm:flex"><strong className="text-xs">{account.username}</strong><small className="text-[.62rem] text-[#8b938c]">{account.role}</small></div>
          </div>
        </header>

        <div className="mx-auto max-w-[1240px] px-4 pt-6 pb-[50px] md:px-[clamp(28px,4vw,58px)] md:pt-[42px] md:pb-[70px]">
          <section className="relative flex min-h-[310px] justify-between overflow-hidden rounded-xl bg-brand-deep p-[30px_26px] text-white md:min-h-[245px] md:p-[42px_45px]">
            <div className="z-1 max-w-[660px]">
              <span className="inline-flex h-[26px] items-center gap-[7px] rounded-full border border-white/15 px-3 text-[.62rem] text-[#c5d2ca]"><span className="size-1.5 rounded-full bg-brand-lime shadow-[0_0_0_4px_rgba(185,244,74,.1)]" />Phiên đang hoạt động</span>
              <h2 className="mt-[26px] mb-2.5 font-serif text-[clamp(2rem,3vw,3rem)] font-normal tracking-[-.045em]">Nền tảng đã sẵn sàng.</h2>
              <p className="max-w-[600px] text-[.85rem] leading-[1.65] text-[#aebfb4]">Auth, API client và cấu trúc feature đã được kết nối. Bạn có thể bắt đầu xây nghiệp vụ riêng ngay từ đây.</p>
            </div>
            <div aria-hidden="true" className="absolute inset-0 flex-[0_0_280px] opacity-50 md:relative md:inset-auto md:opacity-100">
              <span className="absolute -top-[34px] right-[-12px] size-[230px] rounded-full border border-[rgba(185,244,74,.23)]" />
              <span className="absolute top-2 right-[30px] size-[145px] rounded-full border border-dashed border-[rgba(185,244,74,.42)] bg-[rgba(185,244,74,.08)]" />
              <span className="absolute top-[54px] right-[76px] size-[54px] rounded-full bg-brand-lime shadow-[0_0_55px_rgba(185,244,74,.25)]" />
            </div>
          </section>

          <div className="mt-5 grid gap-5 md:grid-cols-[1.25fr_.75fr]">
            <Card className="min-h-[330px] border border-[#dfe2d8] bg-[#fffefb]">
              <Card.Content className="p-[23px_20px] sm:p-[30px]">
                <div className="flex items-center justify-between border-b border-[#e5e8e0] pb-[25px]">
                  <div><p className="text-[.61rem] font-extrabold tracking-[.13em] text-[#959c95] uppercase">Tài khoản</p><h3 className="mt-[7px] text-base font-bold">Thông tin hiện tại</h3></div>
                  <Button variant="outline" size="sm" onPress={handleRefresh} isDisabled={isRefreshing} isPending={isRefreshing} className="min-w-[70px]">
                    {isRefreshing ? <Spinner size="sm" color="current" /> : "Đồng bộ"}
                  </Button>
                </div>
                {notice && (
                  <Alert status="accent" className="mt-3.5">
                    <Alert.Indicator /><Alert.Content><Alert.Description>{notice}</Alert.Description></Alert.Content>
                  </Alert>
                )}
                <dl className="mt-2 mb-0 text-[.78rem]">
                  <div className="grid min-h-[65px] grid-cols-2 items-center border-b border-[#eceee9] sm:grid-cols-[1fr_1.4fr]"><dt className="text-[#858e86]">Tên đăng nhập</dt><dd className="m-0 font-bold">{account.username}</dd></div>
                  <div className="grid min-h-[65px] grid-cols-2 items-center border-b border-[#eceee9] sm:grid-cols-[1fr_1.4fr]"><dt className="text-[#858e86]">Vai trò</dt><dd className="m-0"><span className="rounded-full bg-[#edf6dc] px-2.5 py-1 text-[.62rem] font-bold text-[#567d20]">{account.role}</span></dd></div>
                  <div className="grid min-h-[65px] grid-cols-2 items-center border-b border-[#eceee9] sm:grid-cols-[1fr_1.4fr]"><dt className="text-[#858e86]">Mã tài khoản</dt><dd className="m-0 font-bold">#{account.id.toString().padStart(ACCOUNT_ID_DISPLAY_LENGTH, ACCOUNT_ID_PAD_CHARACTER)}</dd></div>
                </dl>
              </Card.Content>
            </Card>

            <Card className="relative min-h-[330px] overflow-hidden border border-[#dfe2d8] bg-[#fffefb] after:absolute after:-top-20 after:-right-20 after:size-[170px] after:rounded-full after:border-[25px] after:border-[rgba(185,244,74,.12)]">
              <Card.Content className="p-[23px_20px] sm:p-[30px]">
                <div className="mb-6 grid size-[42px] place-items-center rounded-lg bg-[#eaf5d7] text-[#618d21]"><ShieldCheck size={22} /></div>
                <p className="text-[.61rem] font-extrabold tracking-[.13em] text-[#959c95] uppercase">Bảo mật phiên</p>
                <h3 className="mt-[7px] text-base font-bold">Refresh token được bảo vệ</h3>
                <p className="mt-3 mb-[23px] text-xs leading-[1.6] text-[#697269]">Token làm mới nằm trong HttpOnly cookie và không thể bị đọc bởi JavaScript phía trình duyệt.</p>
                {[["HttpOnly cookie", "Đang bật"], ["Refresh khi 401", "Đang bật"]].map(([label, value]) => (
                  <div key={label} className="flex justify-between border-t border-[#eceee9] py-[11px] text-[.68rem] text-[#7c857d]"><span>{label}</span><strong className="text-[.64rem] text-[#568429]">{value}</strong></div>
                ))}
              </Card.Content>
            </Card>
          </div>
        </div>
      </section>
    </main>
  );
}

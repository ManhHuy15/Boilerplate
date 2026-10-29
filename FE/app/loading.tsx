import { Spinner } from "@heroui/react";

export default function Loading() {
  return (
    <main className="grid min-h-screen place-items-center bg-brand-canvas">
      <Spinner size="lg" aria-label="Đang tải trang" />
    </main>
  );
}

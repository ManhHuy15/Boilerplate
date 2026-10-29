"use client";

import { useEffect, type ReactNode } from "react";
import { useAuthStore } from "@/features/auth/hooks/useAuthStore";

export function AuthSessionInitializer({ children }: { children: ReactNode }) {
  const initialize = useAuthStore((state) => state.initialize);

  useEffect(() => {
    void initialize();
  }, [initialize]);

  return children;
}

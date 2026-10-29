"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { AUTH_STATUSES } from "@/features/auth/constants/auth";
import { useAuthStore } from "@/features/auth/hooks/useAuthStore";
import { APP_ROUTES } from "@/shared/constants/routes";
import { ApiClientError } from "@/shared/lib/apiClient";
import { useNotificationStore } from "@/shared/stores/useNotificationStore";

const ACCOUNT_INITIALS_LENGTH = 2;
const SYNC_SUCCESS_MESSAGE = "Thông tin tài khoản đã được đồng bộ.";
const SYNC_ERROR_MESSAGE = "Không thể đồng bộ lúc này. Vui lòng thử lại.";

export function useDashboard() {
  const router = useRouter();
  const showNotification = useNotificationStore((state) => state.showNotification);
  const account = useAuthStore((state) => state.session?.account ?? null);
  const status = useAuthStore((state) => state.status);
  const logout = useAuthStore((state) => state.logout);
  const reloadAccount = useAuthStore((state) => state.reloadAccount);
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [notice, setNotice] = useState<string | null>(null);

  useEffect(() => {
    if (status === AUTH_STATUSES.unauthenticated) {
      router.replace(APP_ROUTES.login);
    }
  }, [router, status]);

  async function handleLogout() {
    setIsLoggingOut(true);
    try {
      await logout();
      showNotification({ severity: "success", message: "Đăng xuất thành công." });
    } catch (error: unknown) {
      showNotification({
        severity: "error",
        message: "Máy chủ không thể hoàn tất đăng xuất.",
        description: error instanceof ApiClientError ? error.message : undefined,
      });
    } finally {
      router.replace(APP_ROUTES.login);
    }
  }

  async function handleRefresh() {
    setIsRefreshing(true);
    setNotice(null);
    try {
      await reloadAccount();
      setNotice(SYNC_SUCCESS_MESSAGE);
      showNotification({ severity: "success", message: SYNC_SUCCESS_MESSAGE });
    } catch (error: unknown) {
      setNotice(SYNC_ERROR_MESSAGE);
      showNotification({
        severity: "error",
        message: SYNC_ERROR_MESSAGE,
        description: error instanceof ApiClientError ? error.message : undefined,
      });
    } finally {
      setIsRefreshing(false);
    }
  }

  return {
    account,
    isLoading: status === AUTH_STATUSES.loading || !account,
    initials: account?.username.slice(0, ACCOUNT_INITIALS_LENGTH).toUpperCase() ?? "",
    isLoggingOut,
    isRefreshing,
    notice,
    handleLogout,
    handleRefresh,
  };
}

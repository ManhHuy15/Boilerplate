"use client";

import { useEffect, useState, type SubmitEvent } from "react";
import { useRouter } from "next/navigation";
import {
  AUTH_CONSTRAINTS,
  AUTH_FIELDS,
  AUTH_FORM_MODES,
  AUTH_STATUSES,
} from "@/features/auth/constants/auth";
import { useAuthStore } from "@/features/auth/hooks/useAuthStore";
import type { AuthFormMode } from "@/features/auth/types/auth";
import { APP_ROUTES } from "@/shared/constants/routes";
import { ApiClientError } from "@/shared/lib/apiClient";
import { useNotificationStore } from "@/shared/stores/useNotificationStore";

interface FormErrors {
  username?: string;
  password?: string;
  form?: string;
}

function validateCredentials(username: string, password: string): FormErrors {
  const errors: FormErrors = {};
  const normalizedUsername = username.trim();

  if (normalizedUsername.length < AUTH_CONSTRAINTS.username.minLength) {
    errors.username = `Tên đăng nhập cần ít nhất ${AUTH_CONSTRAINTS.username.minLength} ký tự.`;
  } else if (normalizedUsername.length > AUTH_CONSTRAINTS.username.maxLength) {
    errors.username = `Tên đăng nhập không được quá ${AUTH_CONSTRAINTS.username.maxLength} ký tự.`;
  }

  if (password.length < AUTH_CONSTRAINTS.password.minLength) {
    errors.password = `Mật khẩu cần ít nhất ${AUTH_CONSTRAINTS.password.minLength} ký tự.`;
  } else if (password.length > AUTH_CONSTRAINTS.password.maxLength) {
    errors.password = `Mật khẩu không được quá ${AUTH_CONSTRAINTS.password.maxLength} ký tự.`;
  }

  return errors;
}

export function useAuthForm(mode: AuthFormMode) {
  const router = useRouter();
  const showNotification = useNotificationStore((state) => state.showNotification);
  const login = useAuthStore((state) => state.login);
  const register = useAuthStore((state) => state.register);
  const status = useAuthStore((state) => state.status);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const isLogin = mode === AUTH_FORM_MODES.login;

  useEffect(() => {
    if (status === AUTH_STATUSES.authenticated) {
      router.replace(APP_ROUTES.dashboard);
    }
  }, [router, status]);

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    const nextErrors = validateCredentials(username, password);
    if (Object.keys(nextErrors).length > 0) {
      setErrors(nextErrors);
      return;
    }

    setErrors({});
    setIsSubmitting(true);

    try {
      const credentials = { username: username.trim(), password };
      await (isLogin ? login(credentials) : register(credentials));
      showNotification({
        severity: "success",
        message: isLogin ? "Đăng nhập thành công." : "Tạo tài khoản thành công.",
      });
      router.replace(APP_ROUTES.dashboard);
    } catch (error: unknown) {
      if (error instanceof ApiClientError) {
        const fieldErrors = error.errors.reduce<FormErrors>((result, apiError) => {
          const field = apiError.field?.toLowerCase();
          if (field === AUTH_FIELDS.username || field === AUTH_FIELDS.password) {
            result[field] = apiError.message;
          }
          return result;
        }, {});
        setErrors({ ...fieldErrors, form: error.message });
        showNotification({
          severity: "error",
          message: isLogin ? "Không thể đăng nhập." : "Không thể tạo tài khoản.",
          description: error.message,
        });
      } else {
        const fallbackMessage = "Đã có lỗi xảy ra. Vui lòng thử lại.";
        setErrors({ form: fallbackMessage });
        showNotification({ severity: "error", message: fallbackMessage });
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    username,
    setUsername,
    password,
    setPassword,
    showPassword,
    togglePasswordVisibility: () => setShowPassword((current) => !current),
    errors,
    isSubmitting: isSubmitting || status === AUTH_STATUSES.loading || status === AUTH_STATUSES.authenticated,
    isLogin,
    handleSubmit,
  };
}

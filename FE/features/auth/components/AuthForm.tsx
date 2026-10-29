"use client";

import Link from "next/link";
import { Alert, Button, FieldError, Input, Label, Spinner, TextField } from "@heroui/react";
import { ArrowRight } from "lucide-react";
import { AUTH_CONSTRAINTS } from "@/features/auth/constants/auth";
import { useAuthForm } from "@/features/auth/hooks/useAuthForm";
import type { AuthFormMode } from "@/features/auth/types/auth";
import { APP_ROUTES } from "@/shared/constants/routes";

interface AuthFormProps {
  mode: AuthFormMode;
}

export function AuthForm({ mode }: AuthFormProps) {
  const {
    username,
    setUsername,
    password,
    setPassword,
    showPassword,
    togglePasswordVisibility,
    errors,
    isSubmitting,
    isLogin,
    handleSubmit,
  } = useAuthForm(mode);

  return (
    <form onSubmit={handleSubmit} noValidate className="grid gap-[22px]">
      {errors.form && (
        <Alert status="danger">
          <Alert.Indicator />
          <Alert.Content><Alert.Description>{errors.form}</Alert.Description></Alert.Content>
        </Alert>
      )}

      <TextField isInvalid={Boolean(errors.username)} isDisabled={isSubmitting}>
        <Label htmlFor="username">Tên đăng nhập</Label>
        <Input
          id="username"
          name="username"
          type="text"
          autoComplete="username"
          value={username}
          onChange={(event) => setUsername(event.target.value)}
          placeholder="vd. minhanh"
          fullWidth
        />
        {errors.username && <FieldError>{errors.username}</FieldError>}
      </TextField>

      <TextField isInvalid={Boolean(errors.password)} isDisabled={isSubmitting}>
        <Label htmlFor="password">Mật khẩu</Label>
        <div className="relative">
          <Input
            id="password"
            name="password"
            type={showPassword ? "text" : "password"}
            autoComplete={isLogin ? "current-password" : "new-password"}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="••••••••"
            className="w-full pr-16"
          />
          <Button
            type="button"
            size="sm"
            variant="ghost"
            onPress={togglePasswordVisibility}
            aria-label={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
            className="absolute top-1/2 right-1 -translate-y-1/2"
          >
            {showPassword ? "Ẩn" : "Hiện"}
          </Button>
        </div>
        <p className="text-xs text-muted">
          {errors.password ?? `Tối thiểu ${AUTH_CONSTRAINTS.password.minLength} ký tự`}
        </p>
        {errors.password && <FieldError>{errors.password}</FieldError>}
      </TextField>

      <Button
        type="submit"
        variant="primary"
        size="lg"
        fullWidth
        isPending={isSubmitting}
        className="mt-0.5 min-h-12 bg-brand-lime text-brand-ink hover:bg-[#c7fa66]"
      >
        {({ isPending }) => (
          <>
            {isPending ? <Spinner size="sm" color="current" /> : <ArrowRight size={17} />}
            {isPending ? "Đang xử lý" : isLogin ? "Đăng nhập" : "Tạo tài khoản"}
          </>
        )}
      </Button>

      <p className="m-0 text-center text-[.78rem] text-[#697269]">
        {isLogin ? "Chưa có tài khoản?" : "Đã có tài khoản?"}{" "}
        <Link className="font-extrabold text-brand-ink underline-offset-3 hover:underline" href={isLogin ? APP_ROUTES.register : APP_ROUTES.login}>
          {isLogin ? "Đăng ký ngay" : "Đăng nhập"}
        </Link>
      </p>
    </form>
  );
}

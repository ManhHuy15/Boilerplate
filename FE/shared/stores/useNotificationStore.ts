"use client";

import { create } from "zustand";

export type NotificationSeverity = "success" | "error" | "warning" | "info";

interface Notification {
  id: number;
  message: string;
  description?: string;
  severity: NotificationSeverity;
}

interface NotificationStore {
  notification: Notification | null;
  showNotification: (notification: Omit<Notification, "id">) => void;
  closeNotification: () => void;
}

export const useNotificationStore = create<NotificationStore>((set) => ({
  notification: null,
  showNotification: (notification) =>
    set({ notification: { ...notification, id: Date.now() } }),
  closeNotification: () => set({ notification: null }),
}));

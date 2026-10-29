"use client";

import { useEffect } from "react";
import { Toast, toast } from "@heroui/react";
import { useNotificationStore } from "@/shared/stores/useNotificationStore";

export function AppToaster() {
  const notification = useNotificationStore((state) => state.notification);
  const closeNotification = useNotificationStore((state) => state.closeNotification);

  useEffect(() => {
    if (!notification) return;

    const options = { description: notification.description };

    switch (notification.severity) {
      case "success":
        toast.success(notification.message, options);
        break;
      case "warning":
        toast.warning(notification.message, options);
        break;
      case "error":
        toast.danger(notification.message, options);
        break;
      default:
        toast.info(notification.message, options);
    }

    closeNotification();
  }, [closeNotification, notification]);

  return <Toast.Provider placement="bottom end" />;
}

import type { Metadata } from "next";
import { DashboardScreen } from "@/features/dashboard/DashboardScreen";

export const dashboardPageMetadata: Metadata = { title: "Dashboard" };

export function DashboardPage() {
  return <DashboardScreen />;
}

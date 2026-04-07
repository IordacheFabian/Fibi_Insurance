import { apiClient, getApiErrorMessage } from "@/lib/axios";
import type { ReportsAnalytics } from "./report.types";

export type ReportsRole = "admin" | "broker";

export interface ReportsAnalyticsFilters {
  from?: string;
  to?: string;
  currency?: string;
  filterByCurrency?: boolean;
}

export async function getReportsAnalytics(role: ReportsRole, filters: ReportsAnalyticsFilters = {}): Promise<ReportsAnalytics> {
  const path = role === "admin"
    ? "/api/admin/reports/analytics"
    : "/api/brokers/reports/analytics";

  try {
    const { data } = await apiClient.get<ReportsAnalytics>(path, {
      params: {
        from: filters.from,
        to: filters.to,
        currency: filters.currency,
        filterByCurrency: filters.filterByCurrency,
      },
    });
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch reports analytics"));
  }
}
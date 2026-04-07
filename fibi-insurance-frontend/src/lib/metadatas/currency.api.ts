import { apiClient, getApiErrorMessage } from "../axios";
import type { PagedResult } from "../clients/client.types";
import type { CurrencyDto } from "./currency.types";

const DEFAULT_PAGE_SIZE = 100;

async function getCurrenciesPage(role: "admin" | "broker" = "admin", pageNumber: number, pageSize: number, isActive = true): Promise<PagedResult<CurrencyDto>> {
  const path = role === "admin" ? "/api/admin/currencies" : "/api/brokers/currencies";

  try {
    const { data } = await apiClient.get<PagedResult<CurrencyDto>>(path, {
      params: {
        isActive,
        pageNumber,
        pageSize,
      },
    });

    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch currencies"));
  }
}

export async function getCurrencies(role: "admin" | "broker" = "admin", pageSize = DEFAULT_PAGE_SIZE): Promise<CurrencyDto[]> {
  const currencies: CurrencyDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCurrenciesPage(role, pageNumber, pageSize, true);
    currencies.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return currencies;
}
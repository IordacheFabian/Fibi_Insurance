import { apiClient, getApiErrorMessage } from "../axios";
import type { PagedResult } from "../clients/client.types";
import type { CurrencyDto } from "./currency.types";

const DEFAULT_PAGE_SIZE = 100;

async function getCurrenciesPage(pageNumber: number, pageSize: number, isActive = true): Promise<PagedResult<CurrencyDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<CurrencyDto>>("/api/admin/currencies", {
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

export async function getCurrencies(pageSize = DEFAULT_PAGE_SIZE): Promise<CurrencyDto[]> {
  const currencies: CurrencyDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCurrenciesPage(pageNumber, pageSize, true);
    currencies.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return currencies;
}
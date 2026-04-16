import { apiClient, getApiErrorMessage } from "../axios";
import type { PagedResult } from "../clients/client.types";
import type { CreateCurrencyRequest, CurrencyDto, UpdateCurrencyRequest } from "./currency.types";

const DEFAULT_PAGE_SIZE = 100;

async function getCurrenciesPage(role: "admin" | "broker" = "admin", pageNumber: number, pageSize: number, isActive?: boolean): Promise<PagedResult<CurrencyDto>> {
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

export async function getCurrencies(role: "admin" | "broker" = "admin", pageSize = DEFAULT_PAGE_SIZE, isActive?: boolean): Promise<CurrencyDto[]> {
  const currencies: CurrencyDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCurrenciesPage(role, pageNumber, pageSize, isActive);
    currencies.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return currencies;
}

export async function createCurrency(payload: CreateCurrencyRequest): Promise<CurrencyDto> {
  try {
    const { data } = await apiClient.post<CurrencyDto>("/api/admin/currencies", payload);
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to create currency"));
  }
}

export async function updateCurrency(currencyId: string, payload: UpdateCurrencyRequest): Promise<CurrencyDto> {
  try {
    const { data } = await apiClient.put<CurrencyDto>(`/api/admin/currencies/${currencyId}`, payload);
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to update currency"));
  }
}
import { apiClient, getApiErrorMessage } from "../axios";
import type { PagedResult } from "../clients/client.types";
import type {
  CreateFeeConfigurationRequest,
  FeeConfigurationDto,
  UpdateFeeConfigurationRequest,
} from "./fee.types";

const DEFAULT_PAGE_SIZE = 100;

async function getFeesPage(pageNumber: number, pageSize: number, isActive?: boolean): Promise<PagedResult<FeeConfigurationDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<FeeConfigurationDto>>("/api/admin/fees", {
      params: {
        isActive,
        pageNumber,
        pageSize,
      },
    });

    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch fee configurations"));
  }
}

export async function getFeeConfigurations(pageSize = DEFAULT_PAGE_SIZE, isActive?: boolean): Promise<FeeConfigurationDto[]> {
  const fees: FeeConfigurationDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getFeesPage(pageNumber, pageSize, isActive);
    fees.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return fees;
}

export async function createFeeConfiguration(payload: CreateFeeConfigurationRequest): Promise<FeeConfigurationDto> {
  try {
    const { data } = await apiClient.post<FeeConfigurationDto>("/api/admin/fees", payload);
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to create fee configuration"));
  }
}

export async function updateFeeConfiguration(feeId: string, payload: UpdateFeeConfigurationRequest): Promise<FeeConfigurationDto> {
  try {
    const { data } = await apiClient.put<FeeConfigurationDto>(`/api/admin/fees/${feeId}`, payload);
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to update fee configuration"));
  }
}
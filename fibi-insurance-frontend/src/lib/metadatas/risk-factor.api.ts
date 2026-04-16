import { apiClient, getApiErrorMessage } from "../axios";
import type { PagedResult } from "../clients/client.types";
import type {
  CreateRiskFactorRequest,
  RiskFactorDto,
  UpdateRiskFactorRequest,
} from "./risk-factor.types";

const DEFAULT_PAGE_SIZE = 100;

async function getRiskFactorsPage(pageNumber: number, pageSize: number, isActive?: boolean): Promise<PagedResult<RiskFactorDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<RiskFactorDto>>("/api/admin/risk-factors", {
      params: {
        isActive,
        pageNumber,
        pageSize,
      },
    });

    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch risk factors"));
  }
}

export async function getRiskFactors(pageSize = DEFAULT_PAGE_SIZE, isActive?: boolean): Promise<RiskFactorDto[]> {
  const riskFactors: RiskFactorDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getRiskFactorsPage(pageNumber, pageSize, isActive);
    riskFactors.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return riskFactors;
}

export async function createRiskFactor(payload: CreateRiskFactorRequest): Promise<RiskFactorDto> {
  try {
    const { data } = await apiClient.post<RiskFactorDto>("/api/admin/risk-factors", payload);
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to create risk factor"));
  }
}

export async function updateRiskFactor(riskFactorId: string, payload: UpdateRiskFactorRequest): Promise<RiskFactorDto> {
  try {
    const { data } = await apiClient.put<RiskFactorDto>(`/api/admin/risk-factors/${riskFactorId}`, payload);
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to update risk factor"));
  }
}
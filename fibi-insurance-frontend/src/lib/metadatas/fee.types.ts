export type FeeTypeValue = number | string;

export interface FeeConfigurationDto {
  id: string;
  name: string;
  feeType: FeeTypeValue;
  percentage: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
}

export interface CreateFeeConfigurationRequest {
  name: string;
  feeType: number;
  percentage: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
}

export interface UpdateFeeConfigurationRequest {
  id: string;
  name: string;
  percentage: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
}
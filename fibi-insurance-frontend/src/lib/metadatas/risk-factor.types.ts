export type RiskLevelValue = number | string;

export interface RiskFactorDto {
  id: string;
  name: string;
  riskLevel: RiskLevelValue;
  referenceId: string | null;
  adjustementPercentage: number;
  isActive: boolean;
}

export interface CreateRiskFactorRequest {
  name: string;
  level: number;
  referenceId: string | null;
  buildingType: number | null;
  adjustementPercentage: number;
  isActive: boolean;
}

export interface UpdateRiskFactorRequest {
  name: string;
  riskLevel: number;
  adjustementPercentage: number;
  isActive: boolean;
}
export interface CurrencyDto {
  id: string;
  code: string;
  name: string;
  exchangeRateToBase: number;
  isActive: boolean;
}

export interface CreateCurrencyRequest {
  code: string;
  name: string;
  exchangeRateToBase: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateCurrencyRequest {
  id: string;
  name: string;
  exchangeRateToBase: number;
  isActive: boolean;
}
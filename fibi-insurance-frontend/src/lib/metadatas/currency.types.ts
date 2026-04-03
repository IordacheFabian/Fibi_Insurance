export interface CurrencyDto {
  id: string;
  code: string;
  name: string;
  exchangeRateToBase: number;
  isActive: boolean;
}
export interface ReportSummary {
  totalWrittenPremium: number;
  totalPremiumRevenue: number;
  totalBrokerEarnings: number;
  claimsRatio: number;
  portfolioGrowth: number;
  collectionRate: number;
  totalPolicies: number;
  totalClaimsIncurred: number;
}

export interface ReportMonthlyPoint {
  month: string;
  premiums: number;
  claims: number;
  newPolicies: number;
}

export interface ReportGeographicPoint {
  region: string;
  policies: number;
  premium: number;
  claims: number;
}

export interface ReportClaimsBreakdownPoint {
  name: string;
  value: number;
}

export interface ReportBrokerPerformancePoint {
  brokerId: string;
  brokerName: string;
  totalPolicies: number;
  activePolicies: number;
  writtenPremium: number;
  collectedPremium: number;
  brokerEarnings: number;
  commissionPercentage: number;
}

export interface ReportsAnalytics {
  currencyCode: string;
  currencyName: string;
  summary: ReportSummary;
  monthly: ReportMonthlyPoint[];
  geographic: ReportGeographicPoint[];
  claimsBreakdown: ReportClaimsBreakdownPoint[];
  brokerPerformance: ReportBrokerPerformancePoint[];
}
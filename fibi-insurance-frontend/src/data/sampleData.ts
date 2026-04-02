export type UserRole = "broker" | "admin";

export interface Client {
  id: string;
  type: "individual" | "company";
  name: string;
  email: string;
  phone: string;
  status: "active" | "inactive" | "pending";
  buildings: number;
  policies: number;
  createdAt: string;
  location: string;
}

export interface Building {
  id: string;
  name: string;
  address: string;
  city: string;
  country: string;
  type: "residential" | "commercial" | "industrial" | "mixed-use";
  insuredValue: number;
  clientId: string;
  clientName: string;
  policyStatus: "active" | "expired" | "none";
  riskLevel: "low" | "medium" | "high";
  yearBuilt: number;
}

export interface Policy {
  id: string;
  policyNumber: string;
  clientName: string;
  buildingName: string;
  status: "draft" | "active" | "expired" | "cancelled";
  premium: number;
  coverageStart: string;
  coverageEnd: string;
  broker: string;
  version: number;
  type: string;
}

export interface Claim {
  id: string;
  claimNumber: string;
  policyNumber: string;
  clientName: string;
  status: "submitted" | "under_review" | "approved" | "rejected";
  amount: number;
  incidentDate: string;
  submittedDate: string;
  type: string;
  description: string;
}

export interface Payment {
  id: string;
  policyNumber: string;
  clientName: string;
  amount: number;
  dueDate: string;
  paidDate: string | null;
  status: "paid" | "pending" | "overdue" | "partial";
  method: string;
}

export interface Activity {
  id: string;
  type: "policy" | "claim" | "payment" | "client" | "endorsement";
  title: string;
  description: string;
  time: string;
  status?: string;
}

export const clients: Client[] = [
  { id: "c1", type: "company", name: "Meridian Holdings Ltd", email: "info@meridian.co", phone: "+44 20 7946 0958", status: "active", buildings: 5, policies: 4, createdAt: "2024-01-15", location: "London, UK" },
  { id: "c2", type: "individual", name: "Sarah Mitchell", email: "sarah.m@email.com", phone: "+44 7911 123456", status: "active", buildings: 2, policies: 2, createdAt: "2024-03-22", location: "Manchester, UK" },
  { id: "c3", type: "company", name: "Atlas Property Group", email: "contact@atlas-prop.com", phone: "+44 20 8123 4567", status: "active", buildings: 12, policies: 10, createdAt: "2023-09-10", location: "Birmingham, UK" },
  { id: "c4", type: "individual", name: "James Rodriguez", email: "j.rodriguez@mail.com", phone: "+44 7700 900123", status: "pending", buildings: 1, policies: 0, createdAt: "2025-01-08", location: "Leeds, UK" },
  { id: "c5", type: "company", name: "Crown Estates International", email: "ops@crownestates.co", phone: "+44 20 3456 7890", status: "active", buildings: 8, policies: 7, createdAt: "2023-06-20", location: "Edinburgh, UK" },
  { id: "c6", type: "individual", name: "Emma Thompson", email: "emma.t@inbox.com", phone: "+44 7911 654321", status: "inactive", buildings: 1, policies: 1, createdAt: "2024-07-12", location: "Bristol, UK" },
  { id: "c7", type: "company", name: "Oakwood Developments", email: "admin@oakwood.dev", phone: "+44 20 9876 5432", status: "active", buildings: 6, policies: 5, createdAt: "2024-02-28", location: "Glasgow, UK" },
  { id: "c8", type: "individual", name: "David Chen", email: "d.chen@email.co", phone: "+44 7800 123789", status: "active", buildings: 3, policies: 3, createdAt: "2024-11-05", location: "Cardiff, UK" },
];

export const buildings: Building[] = [
  { id: "b1", name: "Meridian Tower", address: "42 Canary Wharf", city: "London", country: "UK", type: "commercial", insuredValue: 12500000, clientId: "c1", clientName: "Meridian Holdings Ltd", policyStatus: "active", riskLevel: "low", yearBuilt: 2018 },
  { id: "b2", name: "Mitchell Residence", address: "15 Oak Lane", city: "Manchester", country: "UK", type: "residential", insuredValue: 450000, clientId: "c2", clientName: "Sarah Mitchell", policyStatus: "active", riskLevel: "low", yearBuilt: 2005 },
  { id: "b3", name: "Atlas Business Park", address: "100 Industrial Blvd", city: "Birmingham", country: "UK", type: "industrial", insuredValue: 28000000, clientId: "c3", clientName: "Atlas Property Group", policyStatus: "active", riskLevel: "medium", yearBuilt: 2012 },
  { id: "b4", name: "Crown Plaza", address: "1 Princess St", city: "Edinburgh", country: "UK", type: "mixed-use", insuredValue: 18500000, clientId: "c5", clientName: "Crown Estates International", policyStatus: "active", riskLevel: "low", yearBuilt: 2020 },
  { id: "b5", name: "Rodriguez Flat", address: "88 High Street", city: "Leeds", country: "UK", type: "residential", insuredValue: 320000, clientId: "c4", clientName: "James Rodriguez", policyStatus: "none", riskLevel: "medium", yearBuilt: 1998 },
  { id: "b6", name: "Oakwood Warehouse", address: "22 Dock Road", city: "Glasgow", country: "UK", type: "industrial", insuredValue: 5600000, clientId: "c7", clientName: "Oakwood Developments", policyStatus: "active", riskLevel: "high", yearBuilt: 1995 },
];

export const policies: Policy[] = [
  { id: "p1", policyNumber: "POL-2025-001", clientName: "Meridian Holdings Ltd", buildingName: "Meridian Tower", status: "active", premium: 45000, coverageStart: "2025-01-01", coverageEnd: "2025-12-31", broker: "John Adams", version: 3, type: "Commercial Property" },
  { id: "p2", policyNumber: "POL-2025-002", clientName: "Sarah Mitchell", buildingName: "Mitchell Residence", status: "active", premium: 1200, coverageStart: "2025-03-01", coverageEnd: "2026-02-28", broker: "Lisa Park", version: 1, type: "Residential" },
  { id: "p3", policyNumber: "POL-2025-003", clientName: "Atlas Property Group", buildingName: "Atlas Business Park", status: "active", premium: 85000, coverageStart: "2025-01-15", coverageEnd: "2026-01-14", broker: "John Adams", version: 2, type: "Industrial" },
  { id: "p4", policyNumber: "POL-2024-018", clientName: "Crown Estates International", buildingName: "Crown Plaza", status: "expired", premium: 62000, coverageStart: "2024-01-01", coverageEnd: "2024-12-31", broker: "Mark Stevens", version: 4, type: "Mixed-Use" },
  { id: "p5", policyNumber: "POL-2025-004", clientName: "Crown Estates International", buildingName: "Crown Plaza", status: "active", premium: 67500, coverageStart: "2025-01-01", coverageEnd: "2025-12-31", broker: "Mark Stevens", version: 1, type: "Mixed-Use" },
  { id: "p6", policyNumber: "POL-2025-005", clientName: "Oakwood Developments", buildingName: "Oakwood Warehouse", status: "draft", premium: 18500, coverageStart: "2025-04-01", coverageEnd: "2026-03-31", broker: "Lisa Park", version: 1, type: "Industrial" },
  { id: "p7", policyNumber: "POL-2024-012", clientName: "Emma Thompson", buildingName: "Thompson Cottage", status: "cancelled", premium: 980, coverageStart: "2024-06-01", coverageEnd: "2025-05-31", broker: "John Adams", version: 1, type: "Residential" },
];

export const claims: Claim[] = [
  { id: "cl1", claimNumber: "CLM-2025-0042", policyNumber: "POL-2025-001", clientName: "Meridian Holdings Ltd", status: "under_review", amount: 125000, incidentDate: "2025-02-15", submittedDate: "2025-02-18", type: "Water Damage", description: "Burst pipe on 4th floor causing flooding to offices below." },
  { id: "cl2", claimNumber: "CLM-2025-0039", policyNumber: "POL-2025-003", clientName: "Atlas Property Group", status: "approved", amount: 340000, incidentDate: "2025-01-28", submittedDate: "2025-01-30", type: "Fire Damage", description: "Electrical fire in warehouse section B causing structural damage." },
  { id: "cl3", claimNumber: "CLM-2025-0045", policyNumber: "POL-2025-002", clientName: "Sarah Mitchell", status: "submitted", amount: 8500, incidentDate: "2025-03-10", submittedDate: "2025-03-12", type: "Storm Damage", description: "Roof tiles displaced during storm, water ingress to loft." },
  { id: "cl4", claimNumber: "CLM-2025-0038", policyNumber: "POL-2025-004", clientName: "Crown Estates International", status: "rejected", amount: 15000, incidentDate: "2025-01-20", submittedDate: "2025-01-22", type: "Theft", description: "Break-in at ground floor retail unit. Claim outside coverage scope." },
  { id: "cl5", claimNumber: "CLM-2025-0050", policyNumber: "POL-2025-005", clientName: "Oakwood Developments", status: "submitted", amount: 78000, incidentDate: "2025-03-20", submittedDate: "2025-03-22", type: "Subsidence", description: "Foundation movement detected in north wing of warehouse." },
];

export const payments: Payment[] = [
  { id: "pay1", policyNumber: "POL-2025-001", clientName: "Meridian Holdings Ltd", amount: 11250, dueDate: "2025-03-31", paidDate: "2025-03-28", status: "paid", method: "Bank Transfer" },
  { id: "pay2", policyNumber: "POL-2025-003", clientName: "Atlas Property Group", amount: 21250, dueDate: "2025-04-15", paidDate: null, status: "pending", method: "Direct Debit" },
  { id: "pay3", policyNumber: "POL-2025-002", clientName: "Sarah Mitchell", amount: 300, dueDate: "2025-03-01", paidDate: "2025-03-01", status: "paid", method: "Card" },
  { id: "pay4", policyNumber: "POL-2025-004", clientName: "Crown Estates International", amount: 16875, dueDate: "2025-03-01", paidDate: null, status: "overdue", method: "Bank Transfer" },
  { id: "pay5", policyNumber: "POL-2025-005", clientName: "Oakwood Developments", amount: 4625, dueDate: "2025-04-01", paidDate: null, status: "pending", method: "Bank Transfer" },
  { id: "pay6", policyNumber: "POL-2025-001", clientName: "Meridian Holdings Ltd", amount: 11250, dueDate: "2025-06-30", paidDate: null, status: "pending", method: "Bank Transfer" },
  { id: "pay7", policyNumber: "POL-2025-003", clientName: "Atlas Property Group", amount: 21250, dueDate: "2025-01-15", paidDate: "2025-01-14", status: "paid", method: "Direct Debit" },
  { id: "pay8", policyNumber: "POL-2025-004", clientName: "Crown Estates International", amount: 8000, dueDate: "2025-02-01", paidDate: "2025-02-10", status: "partial", method: "Bank Transfer" },
];

export const recentActivities: Activity[] = [
  { id: "a1", type: "claim", title: "New Claim Submitted", description: "CLM-2025-0050 — Oakwood Developments — Subsidence", time: "2 hours ago", status: "submitted" },
  { id: "a2", type: "payment", title: "Payment Received", description: "£11,250 from Meridian Holdings — POL-2025-001", time: "5 hours ago", status: "paid" },
  { id: "a3", type: "policy", title: "Policy Activated", description: "POL-2025-004 — Crown Estates International", time: "1 day ago", status: "active" },
  { id: "a4", type: "endorsement", title: "Endorsement Created", description: "POL-2025-001 v3 — Premium adjustment", time: "2 days ago" },
  { id: "a5", type: "client", title: "New Client Registered", description: "James Rodriguez — Individual — Leeds, UK", time: "3 days ago" },
  { id: "a6", type: "claim", title: "Claim Approved", description: "CLM-2025-0039 — Atlas Property Group — £340,000", time: "4 days ago", status: "approved" },
  { id: "a7", type: "payment", title: "Payment Overdue", description: "£16,875 from Crown Estates — POL-2025-004", time: "5 days ago", status: "overdue" },
];

export const brokers = [
  { id: "br1", name: "John Adams", email: "j.adams@fibi.com", clients: 12, policies: 18, activePolicies: 14, premiumTotal: 1250000, performance: 94 },
  { id: "br2", name: "Lisa Park", email: "l.park@fibi.com", clients: 8, policies: 11, activePolicies: 9, premiumTotal: 680000, performance: 88 },
  { id: "br3", name: "Mark Stevens", email: "m.stevens@fibi.com", clients: 15, policies: 22, activePolicies: 17, premiumTotal: 1890000, performance: 91 },
];

export const chartDataMonthly = [
  { month: "Jan", premiums: 185000, claims: 45000, newPolicies: 12 },
  { month: "Feb", premiums: 210000, claims: 125000, newPolicies: 8 },
  { month: "Mar", premiums: 195000, claims: 78000, newPolicies: 15 },
  { month: "Apr", premiums: 230000, claims: 32000, newPolicies: 11 },
  { month: "May", premiums: 245000, claims: 95000, newPolicies: 14 },
  { month: "Jun", premiums: 220000, claims: 55000, newPolicies: 9 },
  { month: "Jul", premiums: 260000, claims: 40000, newPolicies: 16 },
  { month: "Aug", premiums: 240000, claims: 110000, newPolicies: 10 },
  { month: "Sep", premiums: 275000, claims: 65000, newPolicies: 13 },
  { month: "Oct", premiums: 290000, claims: 85000, newPolicies: 18 },
  { month: "Nov", premiums: 310000, claims: 50000, newPolicies: 20 },
  { month: "Dec", premiums: 285000, claims: 70000, newPolicies: 12 },
];

export const geographicData = [
  { region: "London", policies: 45, premium: 2800000, claims: 12 },
  { region: "Manchester", policies: 22, premium: 980000, claims: 5 },
  { region: "Birmingham", policies: 18, premium: 1200000, claims: 8 },
  { region: "Edinburgh", policies: 15, premium: 850000, claims: 3 },
  { region: "Glasgow", policies: 12, premium: 620000, claims: 6 },
  { region: "Leeds", policies: 10, premium: 450000, claims: 2 },
  { region: "Bristol", policies: 8, premium: 380000, claims: 4 },
  { region: "Cardiff", policies: 6, premium: 290000, claims: 1 },
];

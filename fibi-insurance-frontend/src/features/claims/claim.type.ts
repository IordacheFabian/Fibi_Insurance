export interface Claim {
    id: string;
    policyId: string;
    policyNumber: string;
    clientName: string;
    // policy
    description: string;
    incidentDate: Date;
    estimatedDamage: number;
    approvedAmount: number;
    status: number;
    rejectionReason: string;
    createdAt: Date;
    reviewedAt?: Date;
    approvedAt?: Date;
    rejectedAt?: Date;
    paidAt?: Date;
}

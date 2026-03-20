export type UserRole = "Admin" | "Broker";

export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthUser {
    id?: string;
    email: string;
    role: UserRole;
    brokerId?: string | null;
}

export interface LoginResponse {
    token: string;
    email: string;
    role: UserRole;
}

export interface MeResponse {
    userId: string;
    email: string;
    role: UserRole;
    brokerId?: string | null;
}


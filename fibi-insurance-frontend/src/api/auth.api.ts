import api from "./axios";
import type { LoginRequest, LoginResponse, AuthUser, MeResponse } from "../features/auth/types";

export const login = async (payload: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>("/auth/login", payload);
    return response.data;
}

export const getMe = async (): Promise<AuthUser> => {
    const response = await api.get<MeResponse>("/auth/me");
    const { userId, email, role, brokerId } = response.data;

    return {
        id: userId,
        email,
        role,
        brokerId: brokerId ?? null,
    };
}
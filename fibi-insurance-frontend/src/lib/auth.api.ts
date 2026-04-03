import { apiClient, getApiErrorMessage, TOKEN_STORAGE_KEY } from "./axios";

export interface LoginRequest {
	email: string;
	password: string;
}

export interface RegisterBrokerRequest {
	email: string;
	password: string;
	brokerId: string;
}

export interface AuthResponse {
	token: string;
	email: string;
	role: string;
}

export interface MeResponse {
	userId: string;
	email: string;
	role: string;
	brokerId?: string;
}

type ApiAuthResponse = Partial<AuthResponse> & {
	Token?: string;
	Email?: string;
	Role?: string;
};

type ApiMeResponse = Partial<MeResponse> & {
	UserId?: string;
	Email?: string;
	Role?: string;
	BrokerId?: string;
};

export function getAuthToken(): string | null {
	if (typeof window === "undefined") {
		return null;
	}

	return window.localStorage.getItem(TOKEN_STORAGE_KEY);
}

export function setAuthToken(token: string): void {
	if (!token || token === "undefined" || token === "null") {
		throw new Error("Auth token is missing from the login response");
	}

	window.localStorage.setItem(TOKEN_STORAGE_KEY, token);
}

export function clearAuthToken(): void {
	if (typeof window === "undefined") {
		return;
	}

	window.localStorage.removeItem(TOKEN_STORAGE_KEY);
}

export function isAuthenticated(): boolean {
	return Boolean(getAuthToken());
}

export async function login(payload: LoginRequest): Promise<AuthResponse> {
	try {
		const { data } = await apiClient.post<ApiAuthResponse>("/api/auth/login", payload);
		const result: AuthResponse = {
			token: data.token ?? data.Token ?? "",
			email: data.email ?? data.Email ?? payload.email,
			role: data.role ?? data.Role ?? "",
		};

		setAuthToken(result.token);
		return result;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Login failed"));
	}
}

export async function registerBroker(payload: RegisterBrokerRequest): Promise<AuthResponse> {
	try {
		const { data } = await apiClient.post<ApiAuthResponse>("/api/auth/register-broker", payload);
		const result: AuthResponse = {
			token: data.token ?? data.Token ?? "",
			email: data.email ?? data.Email ?? payload.email,
			role: data.role ?? data.Role ?? "",
		};

		setAuthToken(result.token);
		return result;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Broker registration failed"));
	}
}

export async function getCurrentUser(): Promise<MeResponse> {
	try {
		const { data } = await apiClient.get<ApiMeResponse>("/api/auth/me");

		return {
			userId: data.userId ?? data.UserId ?? "",
			email: data.email ?? data.Email ?? "",
			role: data.role ?? data.Role ?? "",
			brokerId: data.brokerId ?? data.BrokerId,
		};
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch current user"));
	}
}

export function logout(): void {
	clearAuthToken();
}

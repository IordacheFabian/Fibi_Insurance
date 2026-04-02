const TOKEN_STORAGE_KEY = "fibi.auth.token";

function normalizeApiOrigin(origin: string): string {
	const trimmedOrigin = origin.trim().replace(/\/$/, "");
	const localHttpMatch = trimmedOrigin.match(/^http:\/\/localhost:(\d+)$/i);

	if (localHttpMatch) {
		const port = Number(localHttpMatch[1]);
		if (port >= 7000) {
			return `https://localhost:${port}`;
		}
	}

	return trimmedOrigin;
}

function getApiOrigin(): string {
	const configuredOrigin = typeof import.meta !== "undefined" ? import.meta.env.VITE_API_URL as string | undefined : undefined;

	if (configuredOrigin) {
		return normalizeApiOrigin(configuredOrigin);
	}

	if (typeof window !== "undefined" && window.location.hostname === "localhost" && window.location.port === "8080") {
		return "https://localhost:7260";
	}

	return "";
}

export function resolveApiUrl(path: string): string {
	const apiOrigin = getApiOrigin();
	if (!apiOrigin) {
		return path;
	}

	return `${apiOrigin}${path.startsWith("/") ? path : `/${path}`}`;
}

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

function parseApiError(raw: string, fallback: string): string {
	if (!raw.trim()) {
		return fallback;
	}

	try {
		const parsed = JSON.parse(raw) as { message?: string; title?: string };
		return parsed.message || parsed.title || fallback;
	} catch {
		return raw;
	}
}

async function parseErrorResponse(response: Response, fallback: string): Promise<never> {
	const text = await response.text();
	throw new Error(parseApiError(text, fallback));
}

export function getAuthToken(): string | null {
	return localStorage.getItem(TOKEN_STORAGE_KEY);
}

export function setAuthToken(token: string): void {
	if (!token || token === "undefined" || token === "null") {
		throw new Error("Auth token is missing from the login response");
	}

	localStorage.setItem(TOKEN_STORAGE_KEY, token);
}

export function clearAuthToken(): void {
	localStorage.removeItem(TOKEN_STORAGE_KEY);
}

export function isAuthenticated(): boolean {
	return Boolean(getAuthToken());
}

export async function authFetch(input: RequestInfo | URL, init: RequestInit = {}): Promise<Response> {
	const token = getAuthToken();
	const headers = new Headers(init.headers);

	if (token) {
		headers.set("Authorization", `Bearer ${token}`);
	}

	return fetch(input, {
		...init,
		headers,
	});
}

export async function login(payload: LoginRequest): Promise<AuthResponse> {
	const response = await fetch(resolveApiUrl("/api/auth/login"), {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify(payload),
	});

	if (!response.ok) {
		return parseErrorResponse(response, "Login failed");
	}

	const raw = (await response.json()) as ApiAuthResponse;
	const result: AuthResponse = {
		token: raw.token ?? raw.Token ?? "",
		email: raw.email ?? raw.Email ?? payload.email,
		role: raw.role ?? raw.Role ?? "",
	};

	setAuthToken(result.token);
	return result;
}

export async function registerBroker(payload: RegisterBrokerRequest): Promise<AuthResponse> {
	const response = await fetch(resolveApiUrl("/api/auth/register-broker"), {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify(payload),
	});

	if (!response.ok) {
		return parseErrorResponse(response, "Broker registration failed");
	}

	const raw = (await response.json()) as ApiAuthResponse;
	const result: AuthResponse = {
		token: raw.token ?? raw.Token ?? "",
		email: raw.email ?? raw.Email ?? payload.email,
		role: raw.role ?? raw.Role ?? "",
	};

	setAuthToken(result.token);
	return result;
}

export async function getCurrentUser(): Promise<MeResponse> {
	const response = await authFetch(resolveApiUrl("/api/auth/me"));

	if (!response.ok) {
		if (response.status === 401) {
			clearAuthToken();
		}

		return parseErrorResponse(response, "Failed to fetch current user");
	}

	const raw = (await response.json()) as ApiMeResponse;

	return {
		userId: raw.userId ?? raw.UserId ?? "",
		email: raw.email ?? raw.Email ?? "",
		role: raw.role ?? raw.Role ?? "",
		brokerId: raw.brokerId ?? raw.BrokerId,
	};
}

export function logout(): void {
	clearAuthToken();
}

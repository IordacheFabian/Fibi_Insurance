import axios from "axios";

export const TOKEN_STORAGE_KEY = "fibi.auth.token";

export function normalizeApiOrigin(origin: string): string {
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

export function getApiOrigin(): string {
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

function getStoredAuthToken(): string | null {
	if (typeof window === "undefined") {
		return null;
	}

	return window.localStorage.getItem(TOKEN_STORAGE_KEY);
}

function clearStoredAuthToken(): void {
	if (typeof window === "undefined") {
		return;
	}

	window.localStorage.removeItem(TOKEN_STORAGE_KEY);
}

export const apiClient = axios.create({
	headers: {
		Accept: "application/json",
	},
});

apiClient.interceptors.request.use((config) => {
	if (config.url) {
		config.url = resolveApiUrl(config.url);
	}

	const token = getStoredAuthToken();
	if (token) {
		config.headers.set("Authorization", `Bearer ${token}`);
	}

	return config;
});

apiClient.interceptors.response.use(
	(response) => response,
	(error) => {
		if (axios.isAxiosError(error) && error.response?.status === 401) {
			clearStoredAuthToken();
		}

		return Promise.reject(error);
	},
);

export function getApiErrorMessage(error: unknown, fallback: string): string {
	if (!axios.isAxiosError(error)) {
		return error instanceof Error ? error.message : fallback;
	}

	const data = error.response?.data;

	if (typeof data === "string") {
		if (!data.trim()) {
			return fallback;
		}

		try {
			const parsed = JSON.parse(data) as { message?: string; title?: string; error?: string };
			return parsed.message || parsed.title || parsed.error || data;
		} catch {
			return data;
		}
	}

	if (data && typeof data === "object") {
		const apiError = data as { message?: string; title?: string; error?: string };
		return apiError.message || apiError.title || apiError.error || fallback;
	}

	return error.message || fallback;
}

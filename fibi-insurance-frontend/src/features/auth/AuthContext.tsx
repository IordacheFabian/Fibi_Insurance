import { createContext, useEffect, useMemo, useState, type PropsWithChildren } from "react";
import type { AuthUser, LoginRequest } from "./types";
import { getMe, login as loginRequest } from "../../api/auth.api";

interface AuthContextType {
    user: AuthUser | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (payload: LoginRequest) => Promise<void>;
    logout: () => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children}: PropsWithChildren) {
    const [user, setUser] = useState<AuthUser | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const bootstrapAuth = async () => {
            const token = localStorage.getItem("token");

            if(!token) {
                setIsLoading(false);
                return;
            }

            try {
                const me = await getMe();
                setUser(me);
            } catch {
                localStorage.removeItem("token");
                setUser(null);
            } finally {
                setIsLoading(false);
            }
        };
        
        bootstrapAuth();
    }, []);

    const login = async (payload: LoginRequest) => {
        const data = await loginRequest(payload);
        localStorage.setItem("token", data.token);

        try {
            const me = await getMe();
            setUser(me);
        } catch (error) {
            localStorage.removeItem("token");
            setUser(null);
            throw error;
        }
    }

    const logout = () => {
        localStorage.removeItem("token");
        setUser(null);
    }

    const value = useMemo(
        () => ({
            user,
            isAuthenticated: !!user,
            isLoading,
            login,
            logout,
        }),
        [user, isLoading]
    )

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
import { createContext, useContext, useEffect, useMemo, useState, ReactNode } from "react";
import type { UserRole } from "@/data/sampleData";
import { getAuthToken, getCurrentUser, login, logout, type LoginRequest, type MeResponse } from "@/lib/auth.api";

interface RoleContextType {
  role: UserRole;
  userName: string;
}

interface AuthContextType {
  user: MeResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  signIn: (payload: LoginRequest) => Promise<void>;
  signOut: () => void;
  refreshUser: () => Promise<void>;
}

const RoleContext = createContext<RoleContextType | undefined>(undefined);
const AuthContext = createContext<AuthContextType | undefined>(undefined);

function normalizeRole(role?: string): UserRole {
  return role?.toLowerCase() === "admin" ? "admin" : "broker";
}

function getUserName(user: MeResponse | null): string {
  if (!user?.email) {
    return "User";
  }

  const [namePart] = user.email.split("@");
  if (!namePart) {
    return "User";
  }

  return namePart
    .split(/[._-]+/)
    .filter(Boolean)
    .map((chunk) => chunk[0].toUpperCase() + chunk.slice(1))
    .join(" ");
}

export function RoleProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<MeResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(() => Boolean(getAuthToken()));

  const refreshUser = async () => {
    const token = getAuthToken();
    if (!token) {
      setUser(null);
      return;
    }

    const profile = await getCurrentUser();
    setUser(profile);
  };

  useEffect(() => {
    if (!getAuthToken()) {
      setIsLoading(false);
      return;
    }

    let cancelled = false;

    (async () => {
      try {
        const profile = await getCurrentUser();
        if (!cancelled) {
          setUser(profile);
        }
      } catch {
        if (!cancelled) {
          setUser(null);
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const signIn = async (payload: LoginRequest) => {
    await login(payload);
    await refreshUser();
  };

  const signOut = () => {
    logout();
    setUser(null);
  };

  const role = useMemo(() => normalizeRole(user?.role), [user?.role]);
  const userName = useMemo(() => getUserName(user), [user]);

  const roleContextValue = useMemo(
    () => ({ role, userName }),
    [role, userName],
  );

  const authContextValue = useMemo(
    () => ({
      user,
      isAuthenticated: Boolean(user),
      isLoading,
      signIn,
      signOut,
      refreshUser,
    }),
    [user, isLoading],
  );

  return (
    <AuthContext.Provider value={authContextValue}>
      <RoleContext.Provider value={roleContextValue}>{children}</RoleContext.Provider>
    </AuthContext.Provider>
  );
}

export function useRole() {
  const ctx = useContext(RoleContext);
  if (!ctx) throw new Error("useRole must be used within RoleProvider");
  return ctx;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within RoleProvider");
  return ctx;
}

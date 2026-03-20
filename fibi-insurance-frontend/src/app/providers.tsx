import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { PropsWithChildren } from "react";
import { AuthProvider } from "../features/auth/AuthContext";
import { Toaster } from "react-hot-toast";

const queryClient = new QueryClient();

export default function AppProviders({ children }: PropsWithChildren) {
    return (
        <QueryClientProvider client={queryClient}>
            <AuthProvider>
                {children}
                <Toaster position="top-right" />
            </AuthProvider>
        </QueryClientProvider>
    )
}
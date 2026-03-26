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
                <Toaster
                    position="top-right"
                    toastOptions={{
                        style: {
                            borderRadius: "12px",
                            border: "1px solid #d8e4ef",
                            background: "#ffffff",
                            color: "#182431",
                        },
                    }}
                />
            </AuthProvider>
        </QueryClientProvider>
    )
}
import { createBrowserRouter } from "react-router-dom";
import LoginPage from "../features/auth/LoginPage";
import ProtectedRoute from "../components/common/ProtectedRoute";
import AppLayout from "../components/layout/AppLayout";
import DashboardPage from "../features/dashboard/DashboardPage";
import ClientsPage from "../features/clients/ClientsPage";

export const router = createBrowserRouter([
    {
        path: "/login",
        element: <LoginPage />
    },
    {
        element: <ProtectedRoute />,
        children: [
            {
                element: <AppLayout />,
                children: [
                    {
                        path: "/",
                        element: <DashboardPage />,
                    },
                ],
            },
        ],
    },
    {
        element: <ProtectedRoute allowedRoles={["Broker"]} />,
        children: [
            {
                element: <AppLayout />,
                children: [
                    {
                        path: "/broker/clients",
                        element: <ClientsPage />,
                    }
                ]
            }
        ]
    }
])
import { createBrowserRouter } from "react-router-dom";
import LoginPage from "../features/auth/LoginPage";
import ProtectedRoute from "../components/common/ProtectedRoute";
import AppLayout from "../components/layout/AppLayout";
import DashboardPage from "../features/dashboard/DashboardPage";
import ClientsPage from "../features/clients/ClientsPage";
import NewClientPage from "../features/clients/NewClientPage";
import ClientDetailsPage from "../features/clients/ClientDetailsPage";
import BuildingPage from "../features/buildings/BuildingPage";
import BuildingsDetailsPage from "../features/buildings/BuildingsDetailsPage";

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
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
            path: "/broker/clients/new",
            element: <NewClientPage />,
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
            path: "/broker/clients/:id",
            element: <ClientDetailsPage />,
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
            path: "/broker/clients/:id/buildings",
            element: <BuildingPage />,
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
            path: "/broker/buildings/:id",
            element: <BuildingsDetailsPage />,
          },
        ],
      },
    ],
  },
]);
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Outlet, Route, Routes } from "react-router-dom";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { RoleProvider } from "@/contexts/RoleContext";
import { DashboardLayout } from "@/components/layout/DashboardLayout";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import DashboardPage from "./pages/DashboardPage";
import ClientsPage from "./pages/ClientsPage";
import BuildingsPage from "./pages/BuildingsPage";
import BuildingDetailsPage from "./pages/BuildingDetailsPage";
import CreateBuildingPage from "./pages/CreateBuildingPage";
import EditBuildingPage from "./pages/EditBuildingPage";
import PoliciesPage from "./pages/PoliciesPage";
import ClaimsPage from "./pages/ClaimsPage";
import PaymentsPage from "./pages/PaymentsPage";
import ReportsPage from "./pages/ReportsPage";
import BrokersPage from "./pages/BrokersPage";
import EndorsementsPage from "./pages/EndorsementsPage";
import SettingsPage from "./pages/SettingsPage";
import NotFound from "./pages/NotFound";
import ClientForm from "./pages/ClientForm";
import ClientDetailsPage from "./pages/ClientDetailsPage";
import LoginPage from "./pages/LoginPage";
import CreatePolicyPage from "@/pages/CreatePolicyPage";
import EditClientPage from "@/pages/EditClientPage";
import PolicyDetailsPage from "@/pages/PolicyDetailsPage";

const queryClient = new QueryClient();

const ProtectedLayout = () => (
  <ProtectedRoute>
    <DashboardLayout>
      <Outlet />
    </DashboardLayout>
  </ProtectedRoute>
);

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <RoleProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />

            <Route element={<ProtectedLayout />}>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/clients" element={<ClientsPage />} />
              <Route path="/clients/:id" element={<ClientDetailsPage />} />
              <Route path="/clients/:id/edit" element={<EditClientPage />} />
              <Route path="/clients/new" element={<ClientForm />} />
              <Route path="/buildings" element={<BuildingsPage />} />
              <Route path="/buildings/new" element={<CreateBuildingPage />} />
              <Route path="/buildings/:id" element={<BuildingDetailsPage />} />
              <Route path="/buildings/:id/edit" element={<EditBuildingPage />} />
              <Route path="/policies" element={<PoliciesPage />} />
              <Route path="/policies/new" element={<CreatePolicyPage />} />
              <Route path="/policies/:id" element={<PolicyDetailsPage />} />
              <Route path="/endorsements" element={<EndorsementsPage />} />
              <Route path="/payments" element={<PaymentsPage />} />
              <Route path="/claims" element={<ClaimsPage />} />
              <Route path="/reports" element={<ReportsPage />} />
              <Route path="/brokers" element={<BrokersPage />} />
              <Route path="/settings" element={<SettingsPage />} />
              <Route path="*" element={<NotFound />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </RoleProvider>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;

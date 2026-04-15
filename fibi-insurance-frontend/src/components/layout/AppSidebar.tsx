import { NavLink, useLocation } from "react-router-dom";
import { motion } from "framer-motion";
import {
  LayoutDashboard, Users, Building2, FileText, GitBranch,
  CreditCard, ShieldAlert, BarChart3, UserCog, Settings,
  ChevronLeft, Shield,
} from "lucide-react";
import { useRole } from "@/contexts/RoleContext";
import { cn } from "@/lib/utils";

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

const brokerNav = [
  { label: "Dashboard", icon: LayoutDashboard, path: "/" },
  { label: "Clients", icon: Users, path: "/clients" },
  { label: "Buildings", icon: Building2, path: "/buildings" },
  { label: "Policies", icon: FileText, path: "/policies" },
  { label: "Endorsements", icon: GitBranch, path: "/endorsements" },
  { label: "Payments", icon: CreditCard, path: "/payments" },
  { label: "Claims", icon: ShieldAlert, path: "/claims" },
  { label: "Reports", icon: BarChart3, path: "/reports" },
];

const adminNav = [
  { label: "Dashboard", icon: LayoutDashboard, path: "/" },
  { label: "Brokers", icon: UserCog, path: "/brokers" },
  { label: "Policies", icon: FileText, path: "/policies" },
  { label: "Endorsements", icon: GitBranch, path: "/endorsements" },
  { label: "Claims", icon: ShieldAlert, path: "/claims" },
  { label: "Reports", icon: BarChart3, path: "/reports" },
  { label: "Payments", icon: CreditCard, path: "/payments" },
  { label: "Settings", icon: Settings, path: "/settings" },
];

export function AppSidebar({ collapsed, onToggle }: SidebarProps) {
  const { role } = useRole();
  const location = useLocation();
  const items = role === "admin" ? adminNav : brokerNav;

  return (
    <motion.aside
      initial={false}
      animate={{ width: collapsed ? 72 : 260 }}
      transition={{ duration: 0.3, ease: [0.4, 0, 0.2, 1] }}
      className="fixed left-0 top-0 bottom-0 z-40 flex flex-col border-r border-border bg-card"
    >
      {/* Logo */}
      <div className="flex h-16 items-center justify-between px-4 border-b border-border">
        <div className="flex items-center gap-2.5 overflow-hidden">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg gradient-primary">
            <Shield className="h-5 w-5 text-primary-foreground" />
          </div>
          {!collapsed && (
            <motion.span
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="text-lg font-bold tracking-tight whitespace-nowrap"
            >
              FiBi <span className="text-primary">Insurance</span>
            </motion.span>
          )}
        </div>
        <button
          onClick={onToggle}
          className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
        >
          <ChevronLeft className={cn("h-4 w-4 transition-transform duration-300", collapsed && "rotate-180")} />
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
        {items.map((item) => {
          const isActive = location.pathname === item.path;
          return (
            <NavLink
              key={item.path}
              to={item.path}
              className={cn(
                "group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-200 relative",
                isActive
                  ? "bg-primary/10 text-primary"
                  : "text-muted-foreground hover:bg-muted hover:text-foreground"
              )}
            >
              {isActive && (
                <motion.div
                  layoutId="sidebar-active"
                  className="absolute left-0 top-1/2 -translate-y-1/2 w-0.5 h-5 rounded-full bg-primary"
                  transition={{ duration: 0.2 }}
                />
              )}
              <item.icon className={cn("h-5 w-5 shrink-0", isActive && "text-primary")} />
              {!collapsed && (
                <span className="truncate">{item.label}</span>
              )}
            </NavLink>
          );
        })}
      </nav>

      {/* Footer */}
      {!collapsed && (
        <div className="border-t border-border p-4">
          <div className="text-xs text-muted-foreground">
            FiBi Insurance v2.1
          </div>
        </div>
      )}
    </motion.aside>
  );
}

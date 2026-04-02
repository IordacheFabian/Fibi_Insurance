import { Search, Bell, Moon, Sun, ChevronDown, LogOut } from "lucide-react";
import { useRole } from "@/contexts/RoleContext";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { motion, AnimatePresence } from "framer-motion";
import { useAuth } from "@/contexts/RoleContext";
import { useNavigate } from "react-router-dom";

interface HeaderProps {
  sidebarWidth: number;
}

export function AppHeader({ sidebarWidth }: HeaderProps) {
  const { role, userName } = useRole();
  const { signOut } = useAuth();
  const navigate = useNavigate();
  const [dark, setDark] = useState(true);
  const [showRoleMenu, setShowRoleMenu] = useState(false);

  const handleSignOut = () => {
    signOut();
    setShowRoleMenu(false);
    navigate("/login", { replace: true });
  };

  const toggleTheme = () => {
    setDark(!dark);
    document.documentElement.classList.toggle("dark");
  };

  // Set dark mode on mount
  useState(() => {
    document.documentElement.classList.add("dark");
  });

  return (
    <header
      className="fixed top-0 right-0 z-30 h-16 border-b border-border bg-card/80 backdrop-blur-xl flex items-center justify-between px-6 gap-4"
      style={{ left: sidebarWidth }}
    >
      {/* Search */}
      <div className="flex items-center gap-3 flex-1 max-w-md">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <input
            type="text"
            placeholder="Search clients, policies, claims..."
            className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/50 transition-all"
          />
          <kbd className="absolute right-3 top-1/2 -translate-y-1/2 hidden sm:inline-flex h-5 items-center gap-1 rounded border border-border px-1.5 text-[10px] font-medium text-muted-foreground">
            ⌘K
          </kbd>
        </div>
      </div>

      {/* Right side */}
      <div className="flex items-center gap-2">
        {/* Theme toggle */}
        <button
          onClick={toggleTheme}
          className="flex h-9 w-9 items-center justify-center rounded-lg text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
        >
          {dark ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
        </button>

        {/* Notifications */}
        <button className="relative flex h-9 w-9 items-center justify-center rounded-lg text-muted-foreground hover:bg-muted hover:text-foreground transition-colors">
          <Bell className="h-4 w-4" />
          <span className="absolute top-1.5 right-1.5 h-2 w-2 rounded-full bg-destructive animate-pulse" />
        </button>

        {/* Divider */}
        <div className="w-px h-8 bg-border mx-1" />

        {/* User menu */}
        <div className="relative">
          <button
            onClick={() => setShowRoleMenu(!showRoleMenu)}
            className="flex items-center gap-3 rounded-lg px-3 py-1.5 hover:bg-muted transition-colors"
          >
            <div className="h-8 w-8 rounded-full gradient-primary flex items-center justify-center text-xs font-bold text-primary-foreground">
              {userName.split(" ").map(n => n[0]).join("")}
            </div>
            <div className="hidden md:block text-left">
              <p className="text-sm font-medium leading-tight">{userName}</p>
              <p className="text-xs text-muted-foreground capitalize">{role}</p>
            </div>
            <ChevronDown className="h-3.5 w-3.5 text-muted-foreground" />
          </button>

          <AnimatePresence>
            {showRoleMenu && (
              <>
                <div className="fixed inset-0 z-40" onClick={() => setShowRoleMenu(false)} />
                <motion.div
                  initial={{ opacity: 0, y: -5 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -5 }}
                  className="absolute right-0 top-full mt-2 w-48 rounded-xl border border-border bg-card shadow-lg z-50 overflow-hidden"
                >
                  <div className="p-2 text-xs font-semibold text-muted-foreground uppercase tracking-wider px-3 pt-3">
                    Account
                  </div>
                  <div className="px-3 pb-3 pt-1 text-sm text-muted-foreground capitalize">
                    Signed in as {role}
                  </div>
                  <button
                    onClick={handleSignOut}
                    className="w-full flex items-center gap-2 px-3 py-2 text-sm transition-colors text-foreground hover:bg-muted"
                  >
                    <LogOut className="h-4 w-4" />
                    <span>Sign out</span>
                  </button>
                </motion.div>
              </>
            )}
          </AnimatePresence>
        </div>
      </div>
    </header>
  );
}

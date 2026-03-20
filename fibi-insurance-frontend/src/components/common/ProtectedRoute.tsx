import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

type Props = {
    allowedRoles?: Array<"Admin" | "Broker">;
}

export default function ProtectedRoute({ allowedRoles }: Props) {
    const { user, isAuthenticated, isLoading } = useAuth();

    if(isLoading) {
        return <div className="p-6">Loading...</div>;
    }

    if(!isAuthenticated) {
        return <Navigate to="/login" replace /> 
    }

    if(allowedRoles && user && !allowedRoles.includes(user.role)) {
        return <Navigate to="/" replace />;
    }

    return <Outlet />;    
}
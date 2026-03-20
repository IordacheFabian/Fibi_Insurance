import { Link } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

export default function Sidebar() {
    const { user } = useAuth();

    return (
        <aside className="w-64 border-r bg-white p-4">
            <div className="mb-6 text-lg font-bold">Menu</div>

            <nav className="flex flex-col gap-2">
                <Link to="/" className="rounded px-3 py-2 hover:bg-gray-100">
                    Dashboard                
                </Link>

                {user?.role === "Broker" && (
                    <>
                        <Link to="/broker/clients" className="rounded px-3 py-2 hover:bg-gray-100">
                            Clients
                        </Link>
                        <Link to="/broker/policies" className="rounded px-3 py-2 hover:bg-gray-100">
                            Policies
                        </Link>
                    </>
                )}

                {user?.role === "Admin" && (
                    <>
                        <Link to="admin/brokers" className="rounded px-3 py-2 hover:bg-gray-100">
                            Brokers                        
                        </Link>
                        <Link to="admin/reports/country" className="rounded px-3 py-2 hover:bg-gray-100">
                            Reports
                        </Link>
                    </>
                )}
            </nav>
        </aside>
    )
}
import { useAuth } from "../../hooks/useAuth";

export default function Header() {
    const { user, logout } = useAuth();

    return (
        <header className="flex items-center justify-between border-b bg-white px-6 py-4">
            <h1 className="text-xl font-semibold">FiBI Insurance</h1>

            <div className="flex items-center gap-4">
                <span className="text-sm text-gray-600">
                    {user?.email} ({user?.role})
                </span>
                <button
                    onClick={logout}
                    className="rounded-lg bg-black px-4 py-2 text-white"
                >
                    Logout
                </button>
            </div>
        </header>
    )
}
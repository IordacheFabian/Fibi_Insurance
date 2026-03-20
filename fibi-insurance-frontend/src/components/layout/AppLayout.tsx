import { Outlet } from "react-router-dom"
import Header from "./Header"
import Sidebar from "./Sidebar"

export default function AppLayout() {
    return (
        <div className="min-h-screen bg-gray-100 text-gray-900">
            <div className="flex">
                <Sidebar />
                <div className="flex min-h-screen flex-1 flex-col">
                    <Header />
                    <main className="flex-1 p-6">
                        <Outlet />
                    </main>
                </div>
            </div>
        </div>
    )
}
import { useClients } from "./hooks/useClients";
import { Link } from "react-router";

export default function ClientsPage() {
  const { data: clients, isLoading, isError, error} = useClients();

  if(isLoading) {
    return <div>Loading clients...</div>
  }

  if(isError) {
    return <div>Error loading clients: {error instanceof Error ? error.message : "Unknown error"}</div>
  }

  if(!clients || clients.length === 0) {
    return (
      <div>
        <h2 className="text-2xl font-bold">Clients</h2>
        <p className="mt-2 text-gray-600">No clients found.</p>
      </div>
    )
  }


  return (
    <div>
      <div>
        <h2 className="text-2xl font-bold">Clients</h2>
        <Link
          to="/broker/clients/new"
          className="rounded-lg bg-black px-4 py-2 text-white"
        >
          AddClient
        </Link>
      </div>

      <div className="overflow-hidden rounded-xl border bg-white">
        <table className="min-w-full border-collapse">
          <thead>
            <tr className="border-b bg-gray-50 text-left">
              <th className="px-4 py-3">Name</th>
              <th className="px-4 py-3">Email</th>
              <th className="px-4 py-3">Phone</th>
              <th className="px-4 py-3">City</th>
              <th className="px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody>
            {clients.map((client) => (
              <tr key={client.id} className="border-b last:border-b-0">
                <td className="px-4 py-3">
                  {client.name} 
                </td>
                <td className="px-4 py-3">{client.email}</td>
                <td className="px-4 py-3">{client.phoneNumber}</td>
                <td className="px-4 py-3">
                  <Link
                    to={`/broker/clients/${client.id}`}
                    className="text-sm font-medium text-blue-600 hover:underline"
                  >
                    View Details
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

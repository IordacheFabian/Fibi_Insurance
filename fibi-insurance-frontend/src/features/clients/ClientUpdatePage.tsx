import { useNavigate, useParams } from "react-router-dom";
import toast from "react-hot-toast";
import type { UpdateClientRequest } from "./client.types";
import { useUpdateClient } from "./hooks/useUpdateClient";
import { useClientDetails } from "./hooks/useClientDetails";
import UpdateClientForm from "./ClientUpdateForm";

export default function ClientUpdatePage() {
  const navigate = useNavigate();
  const { id = "" } = useParams<{ id: string }>();
  const { mutateAsync, isPending } = useUpdateClient();
  const { data: client, isLoading, isError, error } = useClientDetails(id);

  const handleSubmit = async (values: UpdateClientRequest) => {
    if (!id) {
      toast.error("Missing client id.");
      return;
    }

    await mutateAsync({ id, payload: values });
    toast.success("Client updated successfully.");
    navigate(`/broker/clients/${id}`);
  };

  if (isLoading) {
    return <div className="state-box loading">Loading client details...</div>;
  }

  if (isError) {
    return (
      <div className="state-box error">
        Error loading client details: {error instanceof Error ? error.message : "Unknown error"}
      </div>
    );
  }

  if (!client) {
    return <div className="state-box empty">Client not found.</div>;
  }

  return (
    <section className="page-surface">
      <div className="page-header">
        <div>
          <h2 className="page-title">Update client</h2>
          <p className="page-subtitle">Update an existing customer profile.</p>
        </div>
      </div>
      <UpdateClientForm
        key={client.id}
        onSubmit={handleSubmit}
        initialValues={{
          name: client.name,
          email: client.email,
          phoneNumber: client.phoneNumber,
        }}
        isSubmitting={isPending}
      />
    </section>
  );
}

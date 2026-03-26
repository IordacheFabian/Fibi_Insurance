import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import ClientForm from "./ClientForm";
import { useCreateClient } from "./hooks/useCreateClient";
import type { CreateClientRequest } from "./client.types";

export default function NewClientPage() {
  const navigate = useNavigate();
  const { mutateAsync, isPending } = useCreateClient();

  const handleSubmit = async (values: CreateClientRequest) => {
    const createdClient = await mutateAsync(values);
    toast.success("Client created successfully.");
    navigate(`/broker/clients/${createdClient.id}`);
  };

  return (
    <section className="page-surface">
      <div className="page-header">
        <div>
          <h2 className="page-title">Add client</h2>
          <p className="page-subtitle">Create a new customer profile.</p>
        </div>
      </div>
      <ClientForm onSubmit={handleSubmit} isSubmitting={isPending} />
    </section>
  );
}

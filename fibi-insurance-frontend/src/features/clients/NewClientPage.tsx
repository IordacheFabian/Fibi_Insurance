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
    <div>
      <h2 className="mb-6 text-2xl font-bold">Add client</h2>
      <ClientForm onSubmit={handleSubmit} isSubmitting={isPending} />
    </div>
  );
}

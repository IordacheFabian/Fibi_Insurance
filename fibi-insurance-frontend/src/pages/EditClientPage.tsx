import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import ClientForm from "./ClientForm";
import { getClientById, updateClient } from "@/lib/clients/client.api";
import type { CreateClient } from "@/lib/clients/client.types";

export default function EditClientPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: client, isLoading, isError, error } = useQuery({
    queryKey: ["clients", id],
    queryFn: () => getClientById(id!),
    enabled: Boolean(id),
    staleTime: 30000,
  });

  const updateClientMutation = useMutation({
    mutationFn: async (values: CreateClient) => {
      if (!id) {
        throw new Error("Client id is missing");
      }

      await updateClient(id, {
        name: values.name,
        email: values.email,
        phoneNumber: values.phoneNumber,
      });
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["clients"] }),
        queryClient.invalidateQueries({ queryKey: ["clients", id] }),
      ]);
      navigate(`/clients/${id}`);
    },
  });

  if (isLoading) {
    return <div className="space-y-6"><p className="text-sm text-muted-foreground">Loading client details...</p></div>;
  }

  if (isError || !client) {
    return <div className="space-y-6"><p className="text-sm text-destructive">{error instanceof Error ? error.message : "Failed to load client"}</p></div>;
  }

  const initialValues: CreateClient = {
    name: client.name,
    clientType: client.clientType === 1 || client.clientType === "company" || client.clientType === "Company" || client.clientType === "1" ? "company" : "individual",
    identification: client.identificationNumber,
    email: client.email,
    phoneNumber: client.phoneNumber,
  };

  return (
    <ClientForm
      mode="edit"
      initialValues={initialValues}
      isSubmitting={updateClientMutation.isPending}
      cancelPath={`/clients/${id}`}
      onSubmit={async (values) => {
        await updateClientMutation.mutateAsync(values);
      }}
    />
  );
}
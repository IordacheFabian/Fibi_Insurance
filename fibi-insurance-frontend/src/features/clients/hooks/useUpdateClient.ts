import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateClient } from "../../../api/clients.api";
import type { UpdateClientRequest } from "../client.types";

export function useUpdateClient() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: UpdateClientRequest }) => updateClient(id, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["clients"] });
        }
    })
}
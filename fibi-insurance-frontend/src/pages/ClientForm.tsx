import { CreateClient } from "@/lib/types";
import { ChangeEvent, FormEvent, useEffect, useState } from "react";
import { createClient } from "@/lib/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { PageHeader } from "@/components/ui/PageHeader";

type Props = {
    onSubmit?: (values: CreateClient) => Promise<void>;
    isSubmitting?: boolean;
    initialValues?: CreateClient;
    mode?: "create" | "edit";
    cancelPath?: string;
};

const initialValues: CreateClient = {
    name: "",
    clientType: "individual",
    identification: "",
    email: "",
    phoneNumber: "",
};

export default function ClientForm({ onSubmit, isSubmitting = false, initialValues: providedInitialValues, mode = "create", cancelPath = "/clients" }: Props) {
    const [form, setForm] = useState<CreateClient>(providedInitialValues ?? initialValues);
    const [error, setError] = useState("");
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    useEffect(() => {
        setForm(providedInitialValues ?? initialValues);
    }, [providedInitialValues]);

    const createClientMutation = useMutation({
        mutationFn: createClient,
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["clients"] });
            navigate("/clients");
        },
    });

    const resolvedIsSubmitting = isSubmitting || createClientMutation.isPending;
    const isEditMode = mode === "edit";

    const handleChange = (field: keyof CreateClient) => (e: ChangeEvent<HTMLInputElement>) => {
        setForm((prev) => ({ ...prev, [field]: e.target.value }));
    };

    const handleClientTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
        const nextType = e.target.value as CreateClient["clientType"];
        setForm((prev) => ({ ...prev, clientType: nextType }));
    };

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        setError("");

        try {
            if (onSubmit) {
                await onSubmit(form);
            } else {
                await createClientMutation.mutateAsync(form);
            }
        } catch (submitError) {
            const message = submitError instanceof Error ? submitError.message : "Failed to create client. Please try again.";
            setError(message);
        }
    };

    return (
        <div className="space-y-6">
            <PageHeader title={isEditMode ? "Update Client" : "Add Client"} description={isEditMode ? "Edit client contact details" : "Create a new client profile"} />

            <form onSubmit={handleSubmit} className="glass-card p-5 space-y-5">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2 md:col-span-2">
                    <label htmlFor="name" className="text-sm font-medium">
                        Full Name
                    </label>
                    <input
                        id="name"
                        type="text"
                        value={form.name}
                        onChange={handleChange("name")}
                        placeholder="e.g. Meridian Holdings Ltd"
                        required
                        disabled={resolvedIsSubmitting}
                        className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                    />
                </div>

                <div className="space-y-2">
                    <label htmlFor="clientType" className="text-sm font-medium">
                        Client Type
                    </label>
                    <select
                        id="clientType"
                        value={form.clientType}
                        onChange={handleClientTypeChange}
                        disabled={resolvedIsSubmitting || isEditMode}
                        className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                    >
                        <option value="individual">Individual</option>
                        <option value="company">Company</option>
                    </select>
                </div>

                <div className="space-y-2">
                    <label htmlFor="identification" className="text-sm font-medium">
                        Identification Number
                    </label>
                    <input
                        id="identification"
                        type="text"
                        value={form.identification}
                        onChange={handleChange("identification")}
                        placeholder="CNP / VAT / National ID"
                        required
                        disabled={resolvedIsSubmitting || isEditMode}
                        className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                    />
                    {isEditMode && (
                        <p className="text-xs text-muted-foreground">Client type and identification number cannot be changed from this screen.</p>
                    )}
                </div>

                <div className="space-y-2">
                    <label htmlFor="email" className="text-sm font-medium">
                        Email
                    </label>
                    <input
                        id="email"
                        type="email"
                        value={form.email}
                        onChange={handleChange("email")}
                        placeholder="name@company.com"
                        required
                        disabled={resolvedIsSubmitting}
                        className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                    />
                </div>

                <div className="space-y-2">
                    <label htmlFor="phoneNumber" className="text-sm font-medium">
                        Phone Number
                    </label>
                    <input
                        id="phoneNumber"
                        type="tel"
                        value={form.phoneNumber}
                        onChange={handleChange("phoneNumber")}
                        placeholder="e.g. +40 712 345 678"
                        required
                        disabled={resolvedIsSubmitting}
                        className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                    />
                </div>
                </div>

                {error && (
                    <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
                        {error}
                    </div>
                )}

                <div className="flex items-center justify-end gap-2 pt-1">
                    <button
                        type="button"
                        onClick={() => navigate(cancelPath)}
                        disabled={resolvedIsSubmitting}
                        className="h-10 px-5 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={resolvedIsSubmitting}
                        className="h-10 px-5 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60 disabled:cursor-not-allowed"
                    >
                        {resolvedIsSubmitting ? (isEditMode ? "Saving..." : "Creating...") : (isEditMode ? "Save Changes" : "Create Client")}
                    </button>
                </div>
            </form>
        </div>
    );
}
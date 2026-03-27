import { useNavigate, useParams } from "react-router-dom";
import toast from "react-hot-toast";
import { useCreateBuilding } from "./hooks/useCreateBuilding";
import type { CreateBuildingRequest } from "./building.type";
import CreateBuildingForm from "./CreateBuildingForm";


export default function NewBuildingPage() {
  const navigate = useNavigate();
  const { id = "" } = useParams<{ id: string }>();
  const { mutateAsync, isPending } = useCreateBuilding();

  const handleSubmit = async (values: CreateBuildingRequest) => {
    if (!id) {
      toast.error("Missing client id.");
      return;
    }

    const createdBuilding = await mutateAsync({
      clientId: id,
      payload: values,
    });
    toast.success("Building created successfully.");
    console.log(createdBuilding);
    navigate(`/broker/clients/${id}/buildings`);
  };

  return (
    <section className="page-surface">
      <div className="page-header">
        <div>
          <h2 className="page-title">Add building</h2>
          <p className="page-subtitle">Create a new building profile.</p>
        </div>
      </div>
      <CreateBuildingForm onSubmit={handleSubmit} isSubmitting={isPending} />
    </section>
  );
}

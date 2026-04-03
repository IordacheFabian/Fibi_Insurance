import { useQuery } from "@tanstack/react-query";
import { AlertCircle, Building2, CalendarDays, LayoutGrid, MapPin, Pencil, Shield, UserRound } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { PageHeader } from "@/components/ui/PageHeader";
import { getBuildingById } from "@/lib/buildings/building.api";
import { BuildingTypeValue } from "@/lib/buildings/building.type";
import { formatMoney } from "@/lib/utils";

function getBuildingTypeLabel(buildingType: BuildingTypeValue): "residential" | "commercial" | "industrial" | "mixed-use" {
	if (typeof buildingType === "number") {
		if (buildingType === 1) {
			return "commercial";
		}

		if (buildingType === 2) {
			return "industrial";
		}

		if (buildingType === 3) {
			return "mixed-use";
		}

		return "residential";
	}

	const normalizedType = buildingType.toLowerCase();

	if (normalizedType === "commercial" || normalizedType === "1") {
		return "commercial";
	}

	if (normalizedType === "industrial" || normalizedType === "2") {
		return "industrial";
	}

	if (normalizedType === "mixeduse" || normalizedType === "mixed-use" || normalizedType === "3") {
		return "mixed-use";
	}

	return "residential";
}

export default function BuildingDetailsPage() {
	const { id } = useParams<{ id: string }>();

	const {
		data: building,
		isLoading,
		isError,
		error,
	} = useQuery({
		queryKey: ["buildings", id],
		queryFn: () => getBuildingById(id!),
		enabled: Boolean(id),
		staleTime: 30000,
	});

	if (isLoading) {
		return <div className="space-y-6"><p className="text-sm text-muted-foreground">Loading building details...</p></div>;
	}

	if (isError || !building) {
		return (
			<div className="space-y-6">
				<div className="flex items-center gap-3 rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-destructive">
					<AlertCircle className="h-5 w-5" />
					<div>
						<p className="font-medium">Could not load building details</p>
						<p className="text-sm">{error instanceof Error ? error.message : "The building details request failed."}</p>
					</div>
				</div>
			</div>
		);
	}

	const fullAddress = `${building.address.street} ${building.address.number}, ${building.address.cityName}`;

	return (
		<div className="space-y-6">
			<PageHeader title="Building Details" description="View this insured property and its owner">
				<Link to={`/buildings/${building.id}/edit`} className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
					<Pencil className="h-4 w-4" /> Edit Building
				</Link>
				<Link to="/buildings" className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors">
					Back to Buildings
				</Link>
			</PageHeader>

			<div className="flex items-center gap-4">
				<div className="h-14 w-14 rounded-2xl bg-primary/10 flex items-center justify-center text-primary">
					<Building2 className="h-7 w-7" />
				</div>
				<div>
					<h2 className="text-2xl font-bold">{fullAddress}</h2>
					<p className="text-sm font-semibold capitalize text-muted-foreground">{getBuildingTypeLabel(building.buildingType)}</p>
				</div>
			</div>

			<div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
				<div className="rounded-lg border border-border bg-card p-4 space-y-3">
					<h3 className="font-semibold">Location</h3>
					<div className="flex items-center gap-2 text-sm text-muted-foreground">
						<MapPin className="h-4 w-4" /> {building.address.street} {building.address.number}
					</div>
					<div className="text-sm text-muted-foreground">City: {building.address.cityName}</div>
					<div className="text-sm text-muted-foreground">City Id: {building.address.cityId}</div>
				</div>

				<div className="rounded-lg border border-border bg-card p-4 space-y-3">
					<h3 className="font-semibold">Structure</h3>
					<div className="flex items-center gap-2 text-sm text-muted-foreground">
						<CalendarDays className="h-4 w-4" /> Built in {building.constructionYear}
					</div>
					<div className="flex items-center gap-2 text-sm text-muted-foreground">
						<LayoutGrid className="h-4 w-4" /> {building.numberOfFloors} floors
					</div>
					<div className="text-sm text-muted-foreground">Surface area: {building.surfaceArea} m2</div>
					<div className="text-sm text-muted-foreground">Type: {getBuildingTypeLabel(building.buildingType)}</div>
				</div>

				<div className="rounded-lg border border-border bg-card p-4 space-y-3">
					<h3 className="font-semibold">Insurance</h3>
					<div className="flex items-center gap-2 text-sm text-muted-foreground">
						<Shield className="h-4 w-4" /> Insured value: {formatMoney(building.insuredValue, building.currencyCode)}
					</div>
					<div className="text-sm text-muted-foreground">Currency: {building.currencyName} ({building.currencyCode})</div>
					<div className="text-sm text-muted-foreground">Risk indicators: {building.riskIndicators || "-"}</div>
				</div>
			</div>

			<div className="grid gap-4 md:grid-cols-2">
				<div className="rounded-lg border border-border bg-card p-4 space-y-3">
					<h3 className="font-semibold">Owner</h3>
					<div className="flex items-center gap-2 text-sm text-muted-foreground">
						<UserRound className="h-4 w-4" /> {building.owner.name}
					</div>
					<div className="text-sm text-muted-foreground">Email: {building.owner.email}</div>
					<div className="text-sm text-muted-foreground">Phone: {building.owner.phoneNumber}</div>
					<div className="text-sm text-muted-foreground">Identification: {building.owner.identificationNumber}</div>
					<Link to={`/clients/${building.owner.id}`} className="inline-flex items-center gap-2 text-sm text-primary hover:underline">
						<Pencil className="h-4 w-4" /> View Owner Details
					</Link>
				</div>

				<div className="rounded-lg border border-border bg-card p-4 space-y-3">
					<h3 className="font-semibold">Owner Portfolio</h3>
					{building.owner.buildings.length === 0 ? (
						<p className="text-sm text-muted-foreground">No buildings registered for this owner.</p>
					) : (
						<div className="space-y-3">
							{building.owner.buildings.map((ownerBuilding) => (
								<Link key={ownerBuilding.id} to={`/buildings/${ownerBuilding.id}`} className="flex items-start gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
									<Building2 className="h-4 w-4 mt-0.5" />
									<div>
										<div>{ownerBuilding.address}</div>
										<div>{ownerBuilding.cityName}</div>
									</div>
								</Link>
							))}
						</div>
					)}
				</div>
			</div>
		</div>
	);
}

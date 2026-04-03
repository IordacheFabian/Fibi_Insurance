export type BuildingTypeValue =
	| number
	| "Residential"
	| "Commercial"
	| "Industrial"
	| "MixedUse"
	| `${number}`;

export interface CreateBuildingAddressDto {
	street: string;
	number: string;
	cityId: string;
}

export interface UpdateBuildingAddressDto {
	street: string;
	number: string;
	cityId: string;
}

export interface AddressDetailsDto {
	id: string;
	cityName: string;
	street: string;
	number: string;
	cityId: string;
}

export interface BuildingListItem {
	id: string;
	address: string;
	cityName: string;
	buildingType: BuildingTypeValue;
	insuredValue: number;
	currencyId: string;
	currencyCode: string;
	currencyName: string;
}

export interface BrokerBuildingListItem extends BuildingListItem {
	constructionYear: number;
	ownerId: string;
	ownerName: string;
}

export interface BuildingOwnerDto {
	id: string;
	type: number | "Individual" | "Company" | "individual" | "company" | `${number}`;
	name: string;
	identificationNumber: string;
	email: string;
	phoneNumber: string;
	buildings: BuildingListItem[];
}

export interface BuildingDetailsDto {
	id: string;
	address: AddressDetailsDto;
	owner: BuildingOwnerDto;
	currencyId: string;
	currencyCode: string;
	currencyName: string;
	constructionYear: number;
	buildingType: BuildingTypeValue;
	numberOfFloors: number;
	surfaceArea: number;
	insuredValue: number;
	riskIndicators: string;
}

export interface CreateBuildingDto {
	clientId: string;
	currencyId: string;
	address: CreateBuildingAddressDto;
	constructionYear: number;
	buildingType: BuildingTypeValue;
	numberOfFloors: number;
	surfaceArea: number;
	insuredValue: number;
	riskIndicators: string;
}

export interface UpdateBuildingDto {
	id: string;
	currencyId: string;
	address?: UpdateBuildingAddressDto;
	constructionYear: number;
	buildingType: BuildingTypeValue;
	numberOfFloors: number;
	surfaceArea: number;
	insuredValue: number;
	riskIndicatiors: string;
}

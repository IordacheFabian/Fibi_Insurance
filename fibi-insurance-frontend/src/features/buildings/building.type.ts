import type { Address } from "../addresses/address.type";
import type { Client } from "../clients/client.types";

export interface Building {
    id: string;
    clientId: string;
    address: Address;
    constructionYear: number;
    buildingType: string;
    numberOfFloors: number;
    surfaceArea: number;
    insuredValue: number;
    riskIndicators: string;

    client: Client;
}

export interface CreateBuildingRequest {
  address: {
    street: string;
    number: string;
    cityId: string;
  };
  constructionYear: number;
  buildingType: number;
  numberOfFloors: number;
  surfaceArea: number;
  insuredValue: number;
  riskIndicators: string;
}

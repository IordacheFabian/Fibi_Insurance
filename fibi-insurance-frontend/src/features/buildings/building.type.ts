import type { Client } from "../clients/client.types";

export interface Building {
    id: string;
    clientId: string;
    //address
    constructionYear: number;
    buildingType: string;
    numberOfFloors: number;
    surfaceArea: number;
    insuredValue: number;
    riskIndicators: string;

    client: Client;
}

export interface CreateBuildingRequest {
  clientId: string;
  constructionYear: number;
  buildingType: string;
  numberOfFloors: number;
  surfaceArea: number;
  insuredValue: number;
  riskIndicators: string;
}
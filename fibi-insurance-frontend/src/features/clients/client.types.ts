import type { Building } from "../buildings/building.type";

export interface Client {
  id: string;
  type: string;
  name: string;
  identificationNumber: string;
  email: string;
  phoneNumber: string;
  buildings: Building[];  
}

export interface CreateClientRequest {
  type: string;
  name: string;
  identificationNumber: string;
  email: string;
  phoneNumber: string;
}

export interface UpdateClientRequest {
  name: string;
  email: string;
  phoneNumber: string;
}

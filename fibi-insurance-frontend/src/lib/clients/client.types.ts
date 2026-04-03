export interface PagedResult<T> {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: T[];
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export type ClientTypeValue = number | "individual" | "company" | "Individual" | "Company" | `${number}`;

export interface ClientBuildingSummary {
  id: string;
  address: string;
  cityName: string;
}

export interface ClientListItem {
  id: string;
  name: string;
  clientType: ClientTypeValue;
  identificationMasked: string;
  email: string;
  phoneNumber: string;
  buildings: ClientBuildingSummary[];
}

export type Client = ClientListItem;

export interface ClientDetails extends Omit<ClientListItem, "identificationMasked"> {
  identificationNumber: string;
}


export interface ClientCreate {
    name: string;
    clientType: ClientTypeValue;
    identification: string;
    email: string;
    phoneNumber: string;
}

export type CreateClientApiRequest = {
  clientType: 0 | 1;
  name: string;
  identificationNumber: string;
  email: string;
  phoneNumber: string;
};

export type CreateClient = Omit<ClientCreate, "clientType"> & {
    clientType: "individual" | "company";
}

export type UpdateClientApiRequest = {
  name: string;
  email: string;
  phoneNumber: string;
};
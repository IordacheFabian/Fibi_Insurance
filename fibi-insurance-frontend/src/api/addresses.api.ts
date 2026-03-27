import type { Address, City, Country, County } from "../features/addresses/address.type";
import api from "./axios";

export const getAddresses = async (): Promise<Address[]> => {
    const response = await api.get("/brokers/addresses");
    return response.data.items;
}

export const getCountries = async (): Promise<Country[]> => {
    const response = await api.get("/brokers/countries");
    return response.data.items;
}

export const getCounties = async (countryId: string): Promise<County[]> => {
    const response = await api.get(`/brokers/countries/${countryId}/counties`);
    return response.data.items;
}

export const getCities = async (countyId: string): Promise<City[]> => {
    const response = await api.get(`/brokers/counties/${countyId}/cities`);
    return response.data.items;
}
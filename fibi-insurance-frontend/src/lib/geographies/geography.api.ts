import { apiClient, getApiErrorMessage } from "../axios";
import type { PagedResult } from "../clients/client.types";
import type { CityDto, CountryDto, CountyDto, GeographyTreeCountry } from "./geography.types";

const DEFAULT_PAGE_SIZE = 100;

export type GeographyRole = "admin" | "broker";

function getGeographyBasePath(role: GeographyRole) {
  return role === "admin" ? "/api/admin" : "/api/brokers";
}

async function getCountriesPage(role: GeographyRole, pageNumber: number, pageSize: number): Promise<PagedResult<CountryDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<CountryDto>>(`${getGeographyBasePath(role)}/countries`, {
      params: { pageNumber, pageSize },
    });
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch countries"));
  }
}

async function getCountiesPage(role: GeographyRole, countryId: string, pageNumber: number, pageSize: number): Promise<PagedResult<CountyDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<CountyDto>>(`${getGeographyBasePath(role)}/countries/${countryId}/counties`, {
      params: { pageNumber, pageSize },
    });
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch counties"));
  }
}

async function getCitiesPage(role: GeographyRole, countyId: string, pageNumber: number, pageSize: number): Promise<PagedResult<CityDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<CityDto>>(`${getGeographyBasePath(role)}/counties/${countyId}/cities`, {
      params: { pageNumber, pageSize },
    });
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch cities"));
  }
}

export async function getCountries(role: GeographyRole = "broker", pageSize = DEFAULT_PAGE_SIZE): Promise<CountryDto[]> {
  const countries: CountryDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCountriesPage(role, pageNumber, pageSize);
    countries.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return countries;
}

export async function getCounties(countryId: string, role: GeographyRole = "broker", pageSize = DEFAULT_PAGE_SIZE): Promise<CountyDto[]> {
  const counties: CountyDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCountiesPage(role, countryId, pageNumber, pageSize);
    counties.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return counties;
}

export async function getCities(countyId: string, role: GeographyRole = "broker", pageSize = DEFAULT_PAGE_SIZE): Promise<CityDto[]> {
  const cities: CityDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCitiesPage(role, countyId, pageNumber, pageSize);
    cities.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return cities;
}

export async function getGeographyTree(role: GeographyRole = "broker"): Promise<GeographyTreeCountry[]> {
  const countries = await getCountries(role);

  return Promise.all(
    countries.map(async (country) => {
      const counties = await getCounties(country.id, role);

      const countiesWithCities = await Promise.all(
        counties.map(async (county) => ({
          ...county,
          cities: await getCities(county.id, role),
        })),
      );

      return {
        ...country,
        counties: countiesWithCities,
      };
    }),
  );
}
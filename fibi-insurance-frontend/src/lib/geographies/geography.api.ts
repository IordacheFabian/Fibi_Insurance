import { apiClient, getApiErrorMessage } from "../axios";
import type { PagedResult } from "../clients/client.types";
import type { CityDto, CountryDto, CountyDto, GeographyTreeCountry } from "./geography.types";

const DEFAULT_PAGE_SIZE = 100;

async function getCountriesPage(pageNumber: number, pageSize: number): Promise<PagedResult<CountryDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<CountryDto>>("/api/brokers/countries", {
      params: { pageNumber, pageSize },
    });
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch countries"));
  }
}

async function getCountiesPage(countryId: string, pageNumber: number, pageSize: number): Promise<PagedResult<CountyDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<CountyDto>>(`/api/brokers/countries/${countryId}/counties`, {
      params: { pageNumber, pageSize },
    });
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch counties"));
  }
}

async function getCitiesPage(countyId: string, pageNumber: number, pageSize: number): Promise<PagedResult<CityDto>> {
  try {
    const { data } = await apiClient.get<PagedResult<CityDto>>(`/api/brokers/counties/${countyId}/cities`, {
      params: { pageNumber, pageSize },
    });
    return data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, "Failed to fetch cities"));
  }
}

export async function getCountries(pageSize = DEFAULT_PAGE_SIZE): Promise<CountryDto[]> {
  const countries: CountryDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCountriesPage(pageNumber, pageSize);
    countries.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return countries;
}

export async function getCounties(countryId: string, pageSize = DEFAULT_PAGE_SIZE): Promise<CountyDto[]> {
  const counties: CountyDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCountiesPage(countryId, pageNumber, pageSize);
    counties.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return counties;
}

export async function getCities(countyId: string, pageSize = DEFAULT_PAGE_SIZE): Promise<CityDto[]> {
  const cities: CityDto[] = [];
  let pageNumber = 1;
  let hasNextPage = true;

  while (hasNextPage) {
    const page = await getCitiesPage(countyId, pageNumber, pageSize);
    cities.push(...page.items);
    hasNextPage = page.hasNextPage;
    pageNumber += 1;
  }

  return cities;
}

export async function getGeographyTree(): Promise<GeographyTreeCountry[]> {
  const countries = await getCountries();

  return Promise.all(
    countries.map(async (country) => {
      const counties = await getCounties(country.id);

      const countiesWithCities = await Promise.all(
        counties.map(async (county) => ({
          ...county,
          cities: await getCities(county.id),
        })),
      );

      return {
        ...country,
        counties: countiesWithCities,
      };
    }),
  );
}
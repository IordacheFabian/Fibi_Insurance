export interface CountryDto {
  id: string;
  name: string;
}

export interface CountyDto {
  id: string;
  name: string;
}

export interface CityDto {
  id: string;
  name: string;
}

export interface GeographyTreeCounty extends CountyDto {
  cities: CityDto[];
}

export interface GeographyTreeCountry extends CountryDto {
  counties: GeographyTreeCounty[];
}
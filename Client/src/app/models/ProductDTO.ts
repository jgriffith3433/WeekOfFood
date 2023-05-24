
export interface ProductDTO {
  id?: number;
  name?: string;
  walmartId?: number | undefined;
  walmartLink?: string | undefined;
  walmartSize?: string | undefined;
  walmartItemResponse?: string | undefined;
  walmartSearchResponse?: string | undefined;
  error?: string | undefined;
  size?: number;
  price?: number;
  verified?: boolean;
  unitType?: number;
  productStockId?: number | undefined;
}

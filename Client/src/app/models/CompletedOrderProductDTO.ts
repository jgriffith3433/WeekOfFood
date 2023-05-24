import { ProductDTO } from "./ProductDTO";

export interface CompletedOrderProductDTO {
  id?: number;
  name?: string;
  walmartId?: number | undefined;
  walmartItemResponse?: string | undefined;
  walmartSearchResponse?: string | undefined;
  walmartError?: string | undefined;
  completedOrderId?: number;
  product?: ProductDTO | undefined;
}

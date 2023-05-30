import { ProductDTO } from "./ProductDTO";

export class CompletedOrderProductDTO {
  id?: number = undefined;
  name?: string = undefined;
  walmartId?: number | undefined = undefined;
  walmartItemResponse?: string | undefined = undefined;
  walmartSearchResponse?: string | undefined = undefined;
  walmartError?: string | undefined = undefined;
  completedOrderId?: number = undefined;
  product?: ProductDTO | undefined = undefined;
}

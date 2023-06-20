import { ProductDTO } from "./ProductDTO";

export class OrderProductDTO {
  id?: number = undefined;
  walmartId?: number = undefined;
  name?: string = undefined;
  product?: ProductDTO | undefined = undefined;
}

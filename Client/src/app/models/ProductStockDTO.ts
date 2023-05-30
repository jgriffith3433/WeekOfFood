import { ProductDTO } from "./ProductDTO";

export class ProductStockDTO {
  id?: number = undefined;
  name?: string = undefined;
  units?: number = undefined;
  productId?: number | undefined = undefined;
  product?: ProductDTO = undefined;
}

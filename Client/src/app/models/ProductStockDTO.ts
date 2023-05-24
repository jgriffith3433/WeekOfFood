import { ProductDTO } from "./ProductDTO";

export interface ProductStockDTO {
  id?: number;
  name?: string;
  units?: number;
  productId?: number | undefined;
  product?: ProductDTO;
}

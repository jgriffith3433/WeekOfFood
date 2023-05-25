import { ProductDTO } from "./ProductDTO";

export interface ProductStockDetailsDTO {
  id?: number;
  name?: string;
  units?: number;
  productId?: number | undefined;
  product?: ProductDTO;
  productSearchItems?: ProductDTO[];
}

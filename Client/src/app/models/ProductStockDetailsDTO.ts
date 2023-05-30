import { ProductDTO } from "./ProductDTO";

export class ProductStockDetailsDTO {
  id?: number = undefined;
  name?: string = undefined;
  units?: number = undefined;
  productId?: number | undefined = undefined;
  product?: ProductDTO;
  productSearchItems?: ProductDTO[] = undefined;
}

import { ProductDTO } from "./ProductDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export interface GetAllProductsVM {
  products: ProductDTO[];
  unitTypes: UnitTypeDTO[];
}

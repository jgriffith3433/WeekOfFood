import { ProductDTO } from "./ProductDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export interface GetAllProductsVm {
  products: ProductDTO[];
  unitTypes: UnitTypeDTO[];
}

import { ProductDTO } from "./ProductDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export class GetAllProductsVM {
  products?: ProductDTO[] = undefined;
  unitTypes?: UnitTypeDTO[] = undefined;
}

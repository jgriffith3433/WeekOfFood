import { ProductStockDTO } from "./ProductStockDTO";

export class CalledIngredientDetailsDTO {
  id?: number = undefined;
  name?: string = undefined;
  productStock?: ProductStockDTO = undefined;
  units?: number | undefined = undefined;
  unitType?: number = undefined;
  productStockId?: number = undefined;
  productStockSearchItems?: ProductStockDTO[] = undefined;
}

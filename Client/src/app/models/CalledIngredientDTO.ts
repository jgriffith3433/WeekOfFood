import { ProductStockDTO } from "./ProductStockDTO";

export class CalledIngredientDTO {
  id?: number = undefined;
  name?: string = undefined;
  productStock?: ProductStockDTO = undefined;
  units?: number | undefined = undefined;
  unitType?: number = undefined;
  productStockId?: number = undefined;
}

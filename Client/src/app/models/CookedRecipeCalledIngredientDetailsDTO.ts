import { CalledIngredientDTO } from "./CalledIngredientDTO";
import { ProductStockDTO } from "./ProductStockDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export class CookedRecipeCalledIngredientDetailsDTO {
  id?: number = undefined;
  cookedRecipeId?: number = undefined;
  calledIngredient?: CalledIngredientDTO = undefined;
  productStock?: ProductStockDTO = undefined;
  productStockId?: number = undefined;
  name?: string = undefined;
  unitType?: UnitTypeDTO = undefined;
  units?: number = undefined;
  productStockSearchItems?: ProductStockDTO[] = undefined;
}

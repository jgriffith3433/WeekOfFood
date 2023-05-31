import { UnitTypeDTO } from "./UnitTypeDTO";

export class UpdateCookedRecipeCalledIngredientDetailsCommand {
  id?: number = undefined;
  unitType?: UnitTypeDTO = undefined;
  productStockId?: number = undefined;
  name?: string = undefined;
  units?: number = undefined;
}

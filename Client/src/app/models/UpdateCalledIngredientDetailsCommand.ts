import { UnitTypeDTO } from "./UnitTypeDTO";

export class UpdateCalledIngredientDetailsCommand {
  id?: number = undefined;
  unitType?: UnitTypeDTO = undefined;
  productStockId?: number = undefined;
  name?: string = undefined;
  units?: number = undefined;
}

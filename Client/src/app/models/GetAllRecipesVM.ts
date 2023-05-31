import { RecipeDTO } from "./RecipeDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export class GetAllRecipesVM {
  recipes?: RecipeDTO[] = undefined;
  unitTypes?: UnitTypeDTO[] = undefined;
}

import { ProductStockDTO } from "./ProductStockDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export class GetAllProductStocksVM {
  productStocks?: ProductStockDTO[] = undefined;
  unitTypes?: UnitTypeDTO[] = undefined;
}

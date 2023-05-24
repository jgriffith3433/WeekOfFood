import { ProductStockDTO } from "./ProductStockDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export interface GetAllProductStocksVM {
  productStocks: ProductStockDTO[];
  unitTypes: UnitTypeDTO[];
}

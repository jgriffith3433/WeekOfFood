import { WalmartProductDTO } from "./WalmartProductDTO";

export class ProductStockDetailsDTO {
  id?: number = undefined;
  name?: string = undefined;
  units?: number = undefined;
  unitType?: number = undefined;
  productId?: number | undefined = undefined;
  walmartProduct?: WalmartProductDTO;
  productSearchItems?: WalmartProductDTO[] = undefined;
}

import { OrderProductDTO } from "./OrderProductDTO";

export class OrderDTO {
  id?: number = undefined;
  orderProducts?: OrderProductDTO[] = undefined;
}

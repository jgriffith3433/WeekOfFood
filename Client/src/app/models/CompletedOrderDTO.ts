import { CompletedOrderProductDTO } from "./CompletedOrderProductDTO";

export interface CompletedOrderDTO {
  id?: number;
  name?: string;
  userImport?: string | undefined;
  completedOrderProducts?: CompletedOrderProductDTO[];
}

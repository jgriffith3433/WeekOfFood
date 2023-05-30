import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CreateProductStockCommand } from "../models/CreateProductStockCommand";
import { GetAllProductStocksVM } from "../models/GetAllProductStocksVM";
import { ProductStockDetailsDTO } from "../models/ProductStockDetailsDTO";
import { ProductStockDTO } from "../models/ProductStockDTO";
import { UpdateProductStockCommand } from "../models/UpdateProductStockCommand";
import { UpdateProductStockDetailsCommand } from "../models/UpdateProductStockDetailsCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class ProductStocksService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll(): Observable<GetAllProductStocksVM> {
    return this.http.get(`${this.baseUri}/ProductStocks`).pipe((map(x => <GetAllProductStocksVM>x)));
  }

  update(id: number | undefined, command: UpdateProductStockCommand): Observable<ProductStockDTO> {
    return this.http.put(`${this.baseUri}/ProductStocks/${id}`, command).pipe((map(x => <ProductStockDTO>x)));
  }

  create(command: CreateProductStockCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/ProductStocks`, command).pipe((map(x => <number>x)));
  }

  getProductStockDetails(id: number | undefined | null, name: string | undefined | null): Observable<ProductStockDetailsDTO> {
    let url = this.baseUri + "/ProductStocks/GetProductStockDetails?";
    if (id === null)
      throw new Error("The parameter 'id' cannot be null.");
    else if (id !== undefined)
      url += "Id=" + encodeURIComponent("" + id) + "&";
    if (name !== undefined && name !== null)
      url += "Name=" + encodeURIComponent("" + name) + "&";
    url = url.replace(/[?&]$/, "");

    return this.http.get(`${url}`).pipe((map(x => <ProductStockDetailsDTO>x)));
  }

  updateProductStockDetails(id: number | undefined, command: UpdateProductStockDetailsCommand): Observable<ProductStockDTO> {
    return this.http.put(`${this.baseUri}/ProductStocks/UpdateProductStockDetails/${id}`, command).pipe((map(x => <ProductStockDTO>x)));
  }

  delete(id: number): Observable<number> {
    return this.http.delete(`${this.baseUri}/ProductStocks/${id}`).pipe((map(x => <number>x)));
  }
}

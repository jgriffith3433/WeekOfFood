import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CreateProductCommand } from "../models/CreateProductCommand";
import { GetAllProductsVM } from "../models/GetAllProductsVM";
import { ProductDetailsDTO } from "../models/ProductDetailsDTO";
import { ProductDTO } from "../models/ProductDTO";
import { UpdateProductCommand } from "../models/UpdateProductCommand";
import { UpdateProductNameCommand } from "../models/UpdateProductNameCommand";
import { UpdateProductSizeCommand } from "../models/UpdateProductSizeCommand";
import { UpdateProductUnitTypeCommand } from "../models/UpdateProductUnitTypeCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll() {
    return this.http.get(`${this.baseUri}/Products`).pipe((map(x => <GetAllProductsVM>x)));
  }

  create(command: any): Observable<number> {
    return this.http.post(`${this.baseUri}/Products`, this._cast(command, CreateProductCommand)).pipe((map(x => <number>x)));
  }

  update(id: number | undefined, command: any): Observable<ProductDTO> {
    return this.http.put(`${this.baseUri}/Products/${id}`, this._cast(command, UpdateProductCommand)).pipe((map(x => <ProductDTO>x)));
  }

  updateName(id: number | undefined, command: any): Observable<ProductDTO> {
    return this.http.put(`${this.baseUri}/Products/UpdateProductName/${id}`, this._cast(command, UpdateProductNameCommand)).pipe((map(x => <ProductDTO>x)));
  }

  updateUnitType(id: number | undefined, command: any): Observable<ProductDTO> {
    return this.http.put(`${this.baseUri}/Products/UpdateUnitType/${id}`, this._cast(command, UpdateProductUnitTypeCommand)).pipe((map(x => <ProductDTO>x)));
  }

  updateSize(id: number | undefined, command: any): Observable<ProductDTO> {
    return this.http.put(`${this.baseUri}/Products/UpdateSize/${id}`, this._cast(command, UpdateProductSizeCommand)).pipe((map(x => <ProductDTO>x)));
  }

  getProductDetails(id: number | undefined): Observable<ProductDTO> {
    let url = this.baseUri + "/Products/GetProductDetails?";
    if (id === null)
      throw new Error("The parameter 'id' cannot be null.");
    else if (id !== undefined)
      url += "Id=" + encodeURIComponent("" + id) + "&";
    url = url.replace(/[?&]$/, "");

    return this.http.get(`${url}`).pipe((map(x => <ProductDetailsDTO>x)));
  }

  delete(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/Products/${id}`).pipe((map(x => <number>x)));
  }

  _cast<K extends T, T>(obj: K, tClass: { new(...args: any[]): K }): K {
    let returnObject: K = new tClass();
    for (let p in returnObject) {
      const value = obj[p] || undefined;
      if (value != undefined) {
        returnObject[p] = value;
      }
    }
    return returnObject;
  }
}

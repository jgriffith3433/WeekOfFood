import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { GetAllProductsVM } from "../models/GetAllProductsVM";
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
}

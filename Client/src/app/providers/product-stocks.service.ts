import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { GetAllProductStocksVM } from "../models/GetAllProductStocksVM";
import { Config } from "./config";

@Injectable({
    providedIn: 'root'
})
export class ProductStocksService {
    constructor(private http: HttpClient) { }

    get baseUri() {
        return `${Config.api}`;
    }

    getAll() {
      return this.http.get(`${this.baseUri}/ProductStocks`).pipe((map(x => <GetAllProductStocksVM>x)));
    }
}

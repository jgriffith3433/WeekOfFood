import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { GetAllCompletedOrdersVm } from "../models/GetAllCompletedOrdersVm";
import { Config } from "./config";

@Injectable({
    providedIn: 'root'
})
export class CompletedOrdersService {
    constructor(private http: HttpClient) { }

    get baseUri() {
        return `${Config.api}`;
    }

    getAll() {
      return this.http.get(`${this.baseUri}/CompletedOrders`).pipe((map(x => <GetAllCompletedOrdersVm>x)));
    }
}

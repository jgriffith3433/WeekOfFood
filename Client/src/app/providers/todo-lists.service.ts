import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { TodoListDTO } from "../models/TodoListDTO";
import { Config } from "./config";

@Injectable({
    providedIn: 'root'
})
export class TodoListsService {
    constructor(private http: HttpClient) { }

    get baseUri() {
        return `${Config.api}`;
    }

    getAll() {
        return this.http.get(`${this.baseUri}/TodoLists`).pipe((map(x => <TodoListDTO[]>x)));
    }
}

import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { GetChatResponseQuery } from "../models/GetChatResponseQuery";
import { Config } from "./config";

@Injectable({
    providedIn: 'root'
})
export class ChatService {
    constructor(private http: HttpClient) { }

    get baseUri() {
        return `${Config.api}`;
    }

    getChatResponse(query: GetChatResponseQuery) {
        return this.http.get(`${this.baseUri}/TodoLists`).pipe((map(x => <TodoListDTO[]>x)));
    }
}

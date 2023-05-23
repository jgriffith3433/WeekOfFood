import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { GetChatResponseQuery } from "../models/GetChatResponseQuery";
import { GetChatResponseVm } from "../models/GetChatResponseVm";
import { GetChatTextFromSpeechVm } from '../models/GetChatTextFromSpeechVm';
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getChatResponse(query: GetChatResponseQuery): Observable<GetChatResponseVm> {
    return this.http.post(`${this.baseUri}/Chat`, query).pipe((map(x => <GetChatResponseVm>x)));
  }

  getChatTextFromSpeech(speech: any): Observable<GetChatTextFromSpeechVm>{
    const formData = new FormData();
    formData.append("speech", speech);
    return this.http.post(`${this.baseUri}/Chat/speech`, formData).pipe((map(x => <GetChatTextFromSpeechVm>x)));;
  }
}

import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { GetChatResponseQuery } from "../models/GetChatResponseQuery";
import { GetChatResponseVm } from "../models/GetChatResponseVm";
import { GetChatTextFromSpeechVm } from '../models/GetChatTextFromSpeechVm';
import { Config } from "./config";
import { DomSanitizer } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  constructor(
    private http: HttpClient, private domSanitizer: DomSanitizer
  ) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getChatResponse(normal: boolean, query: GetChatResponseQuery): Observable<GetChatResponseVm> {
    if (normal) {

    return this.http.post(`${this.baseUri}/Chat/Normal`, query).pipe((map(x => <GetChatResponseVm>x)));
    }
    else {
      return this.http.post(`${this.baseUri}/Chat`, query).pipe((map(x => <GetChatResponseVm>x)));
    }
  }

  getChatTextFromSpeech(speech: any): Observable<GetChatTextFromSpeechVm>{
    const formData = new FormData();
    formData.append("speech", speech);
    return this.http.post(`${this.baseUri}/Chat/Speech`, formData).pipe((map(x => <GetChatTextFromSpeechVm>x)));;
  }

  getChatTextToSpeech(text: string): Observable<string> {
    let url = this.baseUri + "/Chat/TextToSpeech?";
    if (text !== undefined && text !== null)
      url += "Text=" + encodeURIComponent("" + text) + "&";
    url = url.replace(/[?&]$/, "");
    let headers = new HttpHeaders({
      'Accept': 'text/html, application/xhtml+xml, */*',
      'Content-Type': 'application/x-www-form-urlencoded'
    });

    return this.http.get<Blob>(`${url}`, { headers: headers, 'responseType': 'blob' as 'json' }).pipe(map(blob => this.domSanitizer.bypassSecurityTrustUrl(window.URL.createObjectURL(blob)) as string));
  }
}

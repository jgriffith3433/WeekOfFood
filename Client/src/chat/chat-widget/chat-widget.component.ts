import { Component, ElementRef, HostListener, Input, OnInit, ViewChild } from '@angular/core'
import { fadeIn, fadeInOut } from '../animations'
import { NavigationEnd, NavigationStart, Router } from '@angular/router';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, Subject, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { ChatService } from './providers/chat.service';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

import { ChatMessageVm } from '../models/ChatMessageVm'
import { GetChatResponseQuery } from '../models/GetChatResponseQuery'

import { Blob } from 'buffer';

import { blob } from 'node:stream/consumers';

const rand = max => Math.floor(Math.random() * max)

@Component({
  selector: 'chat-widget',
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.css'],
  animations: [fadeInOut, fadeIn],
})
export class ChatWidgetComponent implements OnInit {
  @ViewChild('bottom') bottom: ElementRef;
  @Input() public theme: 'blue' | 'grey' | 'red' = 'blue';
  public _visible = false;
  public _refreshing = false;
  public _botNavigating = false;
  _previousScrollPosition = 0;
  _chatConversationId = undefined;
  previousMessages: ChatMessageVm[] = [];
  private http: HttpClient;
  private baseUrl: string;
  protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;
  mediaRecorder: any;

  constructor(
    private chatClient: ChatClient,
    private router: Router,
    @Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    this.http = http;
    this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : "";
    this.router.events.forEach((event) => {
      // NavigationCancel
      // NavigationError
      // RoutesRecognized
      if (event instanceof NavigationStart) {
        if (!this._refreshing) {
          if (event.url.toLowerCase().indexOf('login') != -1) {
            this.addMessage(this.operator, 'Logging in.', 'received');
          }
          else {
            if (!this._botNavigating) {
              this.addMessage(this.operator, 'Navigating to the ' + (event.url.split('/').join(' ').trim() || 'home') + ' page.', 'received');
            }
          }
        }
      }
      if (event instanceof NavigationEnd) {
        if (this._refreshing) {
          this._refreshing = false;
          //window.scrollTo({ top: this._previousScrollPosition, behavior: 'instant' });
          setTimeout(() => {
            window.scrollTo({ top: this._previousScrollPosition, behavior: 'auto' });
          }, 1000);
        }
        else {
          setTimeout(() => {
            this._chatConversationId = undefined;
            while (this.messages.length > 0) {
              this.messages.pop();
            }
            while (this.previousMessages.length > 0) {
              this.previousMessages.pop();
            }
            if (event.url.toLowerCase().indexOf('login') == -1) {
              this.addMessage(this.operator, 'How can I help you manage your ' + this.getCurrentPageName(), 'received');
            }
            if (this._botNavigating) {
              this._botNavigating = false;
            }
          }, 500);
        }
      }
    });
  }

  public get visible() {
    return this._visible
  }

  @Input() public set visible(visible) {
    this._visible = visible
    if (this._visible) {
      setTimeout(() => {
        this.scrollToBottom()
        //this.focusMessage()
      }, 0)
    }
  }

  /*public focus = new Subject()*/

  public operator = {
    name: 'Operator',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/women/${rand(100)}.jpg`,
  }

  public client = {
    name: 'Guest User',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/men/${rand(100)}.jpg`,
  }

  public messages = []

  public addMessage(from, text, type: 'received' | 'sent') {
    this.messages.unshift({
      from,
      text,
      type,
      date: new Date().getTime(),
    })
    this.scrollToBottom()
  }

  public scrollToBottom() {
    if (this.bottom !== undefined) {
      this.bottom.nativeElement.scrollIntoView()
    }
  }

  //public focusMessage() {
  //  this.focus.next(true)
  //}

  ngOnInit() {
    setTimeout(() => this.visible = true, 2000)
  }

  public toggleChat() {
    this.visible = !this.visible
  }

  getCurrentPageName() {
    return this.router.url.split('/').join(' ').trim() || 'home';
  }

  public sendMessage({ message }) {
    if (message.trim() === '') {
      return;
    }

    let chatMessage: ChatMessageVm = {
      message: message,
      from: 2
    }

    let query: GetChatResponseQuery = {
      chatMessage: chatMessage,
      previousMessages: this.previousMessages,
      chatConversationId: this._chatConversationId,
      currentUrl: this.getCurrentPageName()
    };

    this.chatClient.


    this.chatClient.create(query).subscribe(
      result => {
        if (result.createNewChat) {
          if (result.error) {
            this.addMessage(this.operator, 'System: Something went wrong, creating new chat instance', 'received');
            setTimeout(() => {
              this._chatConversationId = undefined;
              while (this.messages.length > 0) {
                this.messages.pop();
              }
              while (this.previousMessages.length > 0) {
                this.previousMessages.pop();
              }
              this.router.navigateByUrl(this.router.url);
            }, 2000);
          }
          else {
            this._botNavigating = true;
            if (result.responseMessage.message) {
              this.addMessage(this.operator, result.responseMessage.message, 'received');
            }
            setTimeout(() => {
              this._chatConversationId = undefined;
              while (this.messages.length > 0) {
                this.messages.pop();
              }
              while (this.previousMessages.length > 0) {
                this.previousMessages.pop();
              }
              this.router.navigateByUrl(result.navigateToPage);
            }, 2000);
          }
        }
        else {
          this._chatConversationId = result.chatConversationId;
          this.previousMessages = result.previousMessages;
          this.addMessage(this.operator, result.responseMessage.message, 'received');
          if (result.dirty) {
            this._refreshing = true;
            this._previousScrollPosition = window.scrollY || document.getElementsByTagName("html")[0].scrollTop;
            this.router.navigateByUrl(this.router.url);
          }
        }
      },
      error => {
        console.error(error);
        setTimeout(() => {
          this.addMessage(this.operator, 'An error occured, are you logged in?', 'received');
        }, 500);
      }
    );
    this.addMessage(this.client, message, 'sent')
  }

  @HostListener('document:keypress', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.key === '/') {
      //this.focusMessage()
    }
    if (event.key === '?' && !this._visible) {
      this.toggleChat()
    }
  }

  stop() {
    this.mediaRecorder.stop();
  }

  record() {
    const MIN_DECIBELS = -45;

    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
      let endSilenceDetected = false;
      let soundDetected = false;
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.start(3000);

      let audioChunks = [];
      this.mediaRecorder.addEventListener("dataavailable", event => {
        audioChunks.push(event.data);
        const audioBlob = new Blob(audioChunks);
        console.log("sound detected: " + soundDetected);
        console.log("end silence detected: " + endSilenceDetected);
        if (soundDetected && endSilenceDetected) {
          this.mediaRecorder.stop();
        }
      });

      const audioContext = new AudioContext();
      const audioStreamSource = audioContext.createMediaStreamSource(stream);
      const analyser = audioContext.createAnalyser();
      analyser.minDecibels = MIN_DECIBELS;
      audioStreamSource.connect(analyser);

      const bufferLength = analyser.frequencyBinCount;
      const domainData = new Uint8Array(bufferLength);


      const detectSound = () => {
        if (soundDetected) {
          return;
        }

        analyser.getByteFrequencyData(domainData);

        for (let i = 0; i < bufferLength; i++) {
          const value = domainData[i];

          if (domainData[i] > 0) {
            soundDetected = true;
          }
        }
        if (this.mediaRecorder.state == "recording") {
          window.requestAnimationFrame(detectSound);
        }
      };
      window.requestAnimationFrame(detectSound);


      const detectEndSilence = () => {
        analyser.getByteFrequencyData(domainData);

        let endSoundDetected = false;
        for (let i = bufferLength - 1; i >= 0; i--) {
          const value = domainData[i];

          if (domainData[i] > 0) {
            endSoundDetected = true;
          }
          endSilenceDetected = !endSoundDetected && bufferLength - i > 100;
        }

        if (this.mediaRecorder.state == "recording") {
          window.requestAnimationFrame(detectEndSilence);
        }
      };
      window.requestAnimationFrame(detectEndSilence);

      this.mediaRecorder.addEventListener("stop", () => {
        const audioBlob = new Blob(audioChunks);
        if (soundDetected) {
          this.sendSpeech(audioBlob);
          this.record();
        }
        else {
          console.log("no sound detected");
        }
      });
    });
  }

  sendSpeech(speech) {
    let url_ = this.baseUrl + "/api/Chat/speech";
    url_ = url_.replace(/[?&]$/, "");

    const content_ = new FormData();
    content_.append("speech", speech);
    let options_: any = {
      body: content_,
      observe: "response",
      responseType: "blob",
      headers: new HttpHeaders({
        "Accept": "application/json"
      })
    };

    this.http.request("post", url_, options_).pipe(_observableMergeMap((response_: any) => {
      return this.processSpeech(response_);
    })).pipe(_observableCatch((response_: any) => {
      if (response_ instanceof HttpResponseBase) {
        try {
          return this.processSpeech(response_ as any);
        } catch (e) {
          return _observableThrow(e) as any as Observable<GetChatTextFromSpeechVm>;
        }
      } else
        return _observableThrow(response_) as any as Observable<GetChatTextFromSpeechVm>;
    })).subscribe(
      result => {
        var message = result.text;
        this.sendMessage({ message } as any);
      },
      error => {
        console.error(error);
        setTimeout(() => {
          this.addMessage(this.operator, 'An error while transcribing audio.', 'received');
        }, 500);
      }
    );
  }

  protected processSpeech(response: HttpResponseBase): Observable<GetChatTextFromSpeechVm> {
    const status = response.status;
    const responseBlob =
      response instanceof HttpResponse ? response.body :
        (response as any).error instanceof Blob ? (response as any).error : undefined;

    let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } }
    if (status === 200) {
      return this.blobToText(responseBlob).pipe(_observableMergeMap((_responseText: string) => {
        let result200: any = null;
        let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
        result200 = GetChatTextFromSpeechVm.fromJS(resultData200);
        return _observableOf(result200);
      }));
    } else if (status !== 200 && status !== 204) {
      return this.blobToText(responseBlob).pipe(_observableMergeMap((_responseText: string) => {
        return this.throwException("An unexpected server error occurred.", status, _responseText, _headers);
      }));
    }
    return _observableOf(null as any);
  }

  blobToText(blob: any): Observable<string> {
    return new Observable<string>((observer: any) => {
      if (!blob) {
        observer.next("");
        observer.complete();
      } else {
        let reader = new FileReader();
        reader.onload = event => {
          observer.next((event.target as any).result);
          observer.complete();
        };
        reader.readAsText(blob);
      }
    });
  }

  throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): Observable<any> {
    if (result !== null && result !== undefined)
      return _observableThrow(result);
    else
      return _observableThrow(new SwaggerException(message, status, response, headers, null));
  }
}
//declare class MediaStreamRecorder {
//  constructor(stream);
//  mimeType: string;
//  ondataavailable(blob);
//  start(time);
//};

//interface Navigator {
//  getUserMedia(
//    options: { video?: boolean; audio?: boolean; },
//    success: (stream: any) => void,
//    error?: (error: string) => void
//  ): void;
//}

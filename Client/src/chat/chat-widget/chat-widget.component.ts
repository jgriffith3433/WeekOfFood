import { Component, ElementRef, HostListener, Input, OnInit, ViewChild } from '@angular/core'
import { fadeIn, fadeInOut } from '../animations'
import { NavigationEnd, NavigationStart, Router } from '@angular/router';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, Subject, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { ChatService } from '../providers/chat.service'

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

import { ChatMessageVm } from '../models/ChatMessageVm'
import { GetChatResponseQuery } from '../models/GetChatResponseQuery'
import { GetChatTextFromSpeechVm } from '../models/GetChatTextFromSpeechVm';

//read online that maybe you want to use this instead?
//import { Blob } from 'buffer';


const rand = (max: number) => Math.floor(Math.random() * max)

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
  _chatConversationId: number = -1;
  chatMessages: ChatMessageVm[] = [];
  private http: HttpClient;
  private baseUrl: string;
  protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;
  mediaRecorder: any;
  keepRecording:boolean = false;

  constructor(
    private chatService: ChatService,
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
            this._chatConversationId = -1;
            while (this.messages.length > 0) {
              this.messages.pop();
            }
            while (this.chatMessages.length > 0) {
              this.chatMessages.pop();
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

  public messages: any[] = []

  public addMessage(from: any, text: any, type: 'received' | 'sent') {
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

  public sendMessage({ message }: any) {
    if (message.trim() === '') {
      return;
    }

    let chatMessage: ChatMessageVm = {
      content: message,
      name: "user",
      role: "user"
    }

    this.chatMessages.push(chatMessage);

    let query: GetChatResponseQuery = {
      sendToRole: "assistant",
      chatMessages: this.chatMessages,
      chatConversationId: this._chatConversationId,
      currentUrl: this.getCurrentPageName()
    };

    this.chatService.getChatResponse(query).subscribe(
      result => {
        if (result.createNewChat) {
          if (result.error) {
            this.addMessage(this.operator, 'System: Something went wrong, creating new chat instance', 'received');
            setTimeout(() => {
              this._chatConversationId = -1;
              while (this.messages.length > 0) {
                this.messages.pop();
              }
              while (this.chatMessages.length > 0) {
                this.chatMessages.pop();
              }
              this.router.navigateByUrl(this.router.url);
            }, 2000);
          }
          else {
            this._botNavigating = true;
            while (this.messages.length > 0) {
              this.messages.pop();
            }
            if (result.chatMessages) {
              for (let m of result.chatMessages) {
                this.addMessage(this.operator, m.content, 'received');
              }
            }
            setTimeout(() => {
              this._chatConversationId = -1;
              while (this.messages.length > 0) {
                this.messages.pop();
              }
              while (this.chatMessages.length > 0) {
                this.chatMessages.pop();
              }
              this.router.navigateByUrl(result.navigateToPage || '/');
            }, 2000);
          }
        }
        else {
          this._chatConversationId = result.chatConversationId || -1;
          while (this.messages.length > 0) {
            this.messages.pop();
          }
          if (result.chatMessages) {
            for (let m of result.chatMessages) {
              this.addMessage(this.operator, m.content, 'received');
            }
          }

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
    this.keepRecording = false;
    this.mediaRecorder.stop();
  }

  record() {
    this.keepRecording = true;
    const MIN_DECIBELS = -45;

    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
      let endSilenceDetected = false;
      let soundDetected = false;
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.start(3000);

      let audioChunks: any[] = [];
      this.mediaRecorder.addEventListener("dataavailable", (event: { data: any; }) => {
        audioChunks.push(event.data);
        const audioBlob = new Blob(audioChunks);
        console.log("sound detected: " + soundDetected);
        console.log("end silence detected: " + endSilenceDetected);
        if (soundDetected && endSilenceDetected) {
          this.keepRecording = false;
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
          if (this.keepRecording) {
            this.record();
          }
        }
        else {
          console.log("no sound detected");
        }
      });
    });
  }

  sendSpeech(speech: any) {
    this.chatService.getChatTextFromSpeech(speech).subscribe(
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

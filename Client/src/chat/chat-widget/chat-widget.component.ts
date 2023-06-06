import { Component, ElementRef, HostListener, Input, OnDestroy, OnInit, ViewChild } from '@angular/core'
import { fadeIn, fadeInOut } from '../animations'
import { NavigationEnd, NavigationStart, Router } from '@angular/router';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, Subject, throwError as _observableThrow, of as _observableOf, Subscription } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { ChatService } from '../providers/chat.service'

interface RecommendedVoices {
  [key: string]: boolean;
}

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

import { ChatMessageVm } from '../models/ChatMessageVm'
import { GetChatResponseQuery } from '../models/GetChatResponseQuery'
import { GetChatTextFromSpeechVm } from '../models/GetChatTextFromSpeechVm';
import { TokenService } from '../../app/providers/token.service';
import { PicoService } from '../providers/pico.service';
import { GetChatResponseVm } from '../models/GetChatResponseVm';

//read online that maybe you want to use this instead?
//import { Blob } from 'buffer';


const rand = (max: number) => Math.floor(Math.random() * max)

@Component({
  selector: 'chat-widget',
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.css'],
  animations: [fadeInOut, fadeIn],
})
export class ChatWidgetComponent implements OnInit, OnDestroy {
  @ViewChild('bottom') bottom: ElementRef;
  @ViewChild('audio') audioPlayerRef: ElementRef;
  @Input() public theme: 'blue' | 'grey' | 'red' = 'blue';
  public _visible = false;
  public _refreshing = false;
  public _botNavigating = false;
  greeting: string;
  _previousScrollPosition = 0;
  _chatConversationId: number = -1;
  chatMessages: ChatMessageVm[] = [];
  private http: HttpClient;
  private baseUrl: string;
  protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;
  mediaRecorder: MediaRecorder;
  keepRecording: boolean = false;
  lastTimeDetected: number;
  timeSinceDetected: number;
  soundDetectedSendToServer: boolean = false;
  soundWasDetected: boolean = false;
  //endSilenceDetected: boolean = false;
  public speechSynthesisOn: boolean = true;
  public normalConversation: boolean = false;
  audioSource = '';
  private keywordSubscription: Subscription;
  private errorSubscription: Subscription;

  constructor(
    private chatService: ChatService,
    private router: Router,
    private tokenService: TokenService,
    private picoService: PicoService,
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
            this.greeting = 'Logging in.';
          }
          else {
            if (!this._botNavigating) {
              this.greeting = 'Navigating to the ' + (event.url.split('/').join(' ').trim() || 'home') + ' page.';
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
          //setTimeout(() => {
          //  this._chatConversationId = -1;
          //  if (event.url.toLowerCase().indexOf('login') == -1) {
          //    while (this.messages.length > 0) {
          //      this.messages.pop();
          //    }
          //    while (this.chatMessages.length > 0) {
          //      this.chatMessages.pop();
          //    }
          //    //this.greeting = 'How can I help you manage your ' + this.getCurrentPageName();
          //    this.greeting = 'Try saying: bumblebee';
          //  }
          //  if (this._botNavigating) {
          //    this._botNavigating = false;
          //  }
          //}, 500);
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

  public get recording() {
    return this.mediaRecorder && this.mediaRecorder.state == "recording";
  }

  /*public focus = new Subject()*/

  public assistant = {
    name: 'assistant',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/women/${rand(100)}.jpg`,
  }

  public system = {
    name: 'system',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/women/${rand(100)}.jpg`,
  }

  public user = {
    name: 'user',
    status: 'online',
    avatar: `https://randomuser.me/api/portraits/men/${rand(100)}.jpg`,
  }

  public messages: any[] = [];


  public getAvatarForRole(role: string | undefined): string {
    if (role == 'user') {
      return this.user.avatar;
    }
    else if (role == 'system') {
      return this.system.avatar;
    }
    else if (role == 'assistant') {
      return this.assistant.avatar;
    }
    return "";
  }

  public addMessage(newChatMessage: ChatMessageVm) {
    this.chatMessages.push(newChatMessage);
  }

  public scrollToBottom() {
    if (this.bottom !== undefined) {
      this.bottom.nativeElement.scrollIntoView()
    }
  }

  //public focusMessage() {
  //  this.focus.next(true)
  //}

  async ngOnInit() {
    this.keywordSubscription = this.picoService.keywordDetectionListener.subscribe(porcupineSubscription => {
      console.log('wake word: ' + porcupineSubscription.label);
      if (!this.visible) {
        this.toggleChat();
      }
      else {
        if (!this.recording) {
          this.record(false);
        }
      }
    });
    this.errorSubscription = this.picoService.errorListener.subscribe(error => {
      if (error) {
        console.error(error);
      }
    });
    this.picoService.autoStart = true;
    this.picoService.load();
  }

  ngOnDestroy(): void {
    this.keywordSubscription.unsubscribe();
    this.errorSubscription.unsubscribe();
  }

  public toggleChat() {
    if (!this.tokenService.IsAuthenticated) {
      this.router.navigate(['login']);
      return;
    }
    this.visible = !this.visible;
    if (this.visible) {
      if (this.tokenService.IsPicoAuthenticated) {
        if (!this.picoService.isLoaded) {
          this.picoService.autoStart = true;
          this.picoService.load();
        }
        else {
          if (!this.picoService.isListening) {
            this.picoService.start();
          }
        }
      }
      this.record(false);
    }
    else {
      if (this.recording) {
        this.stopRecording();
      }
    }
  }

  getCurrentPageName() {
    return this.router.url.split('/').join(' ').trim() || 'home';
  }

  public sendMessage({ message }: any) {
    if (message.trim() === '') {
      return;
    }

    this.addMessage({
      content: message,
      rawContent: message,
      name: this.user.name,
      role: this.user.name
    } as ChatMessageVm);
    this.scrollToBottom();

    let query: GetChatResponseQuery = {
      sendToRole: "assistant",
      chatMessages: this.chatMessages,
      chatConversationId: this._chatConversationId,
      currentUrl: this.getCurrentPageName(),
    };

    this.chatService.getChatResponse(this.normalConversation, query).subscribe(
      result => this.receiveMessage(result),
      error => {
        setTimeout(() => {
          if (this.getCurrentPageName() != 'login') {
            if (error.errors) {
              for (let e in error.errors) {
                for (let i in error.errors[e]) {
                  this.addMessage({
                    content: error.errors[e][i],
                    rawContent: error.errors[e][i],
                    name: this.system.name,
                    role: this.system.name
                  } as ChatMessageVm);
                }
                console.error(error.errors[e]);
              }
            }
            else if (error.message) {
              this.addMessage({
                content: error.message,
                rawContent: error.message,
                name: this.system.name,
                role: this.system.name
              } as ChatMessageVm);
            }
            this.scrollToBottom();
          }
        }, 500);
      }
    );
  }

  getSystemMessage() {
    for (let i = this.chatMessages.length - 1; i >= 0; i--) {
      if (this.chatMessages[i].role == this.system.name) {
        return this.chatMessages[i].content;
      }
    }
    return this.greeting;
  }



  receiveMessage(response: GetChatResponseVm) {
    let newChatMessages: ChatMessageVm[] = [];
    if (response.chatMessages) {
      for (let i = this.chatMessages.length; i < response.chatMessages.length; i++) {
        newChatMessages.push(response.chatMessages[i]);
      }
    }
    if (this.speechSynthesisOn) {
      let textToSpeak = "";
      //for (let c of newChatMessages) {
      //  if (c.content && c.role == this.assistant.name) {
      //    textToSpeak += c.content + '...';
      //  }
      //}
      if (newChatMessages.length > 0) {
        let mostRecentMessage = newChatMessages[newChatMessages.length - 1];
        if (mostRecentMessage.content && mostRecentMessage.role == this.assistant.name) {
          textToSpeak = mostRecentMessage.content;
        }
      }
      if (textToSpeak && textToSpeak.indexOf('Rate limit reached') == -1) {
        this.chatService.getChatTextToSpeech(textToSpeak).subscribe(
          result => {
            this.audioSource = result;
            let audioPlayer = this.audioPlayerRef.nativeElement;
            audioPlayer.crossOrigin = "anonymous";
            audioPlayer.addEventListener("canplaythrough", function () {
              audioPlayer.play();
            })
          });
      }
    }
    if (response.createNewChat) {
      if (response.error) {
        this.greeting = 'Something went wrong, creating new chat instance';
        setTimeout(() => {
          this._chatConversationId = -1;
          while (this.chatMessages.length > 0) {
            this.chatMessages.pop();
          }
          this.router.navigateByUrl(this.router.url);
        }, 2000);
      }
      else {
        this._botNavigating = true;
        for (let newChatMessage of newChatMessages) {
          this.addMessage(newChatMessage);
        }
        this.scrollToBottom();
        setTimeout(() => {
          this._chatConversationId = -1;
          while (this.chatMessages.length > 0) {
            this.chatMessages.pop();
          }
          this.router.navigateByUrl(response.navigateToPage || '/');
        }, 2000);
      }
    }
    else {
      this._chatConversationId = response.chatConversationId || -1;
      for (let newChatMessage of newChatMessages) {
        this.addMessage(newChatMessage);
      }
      this.scrollToBottom();
      if (response.navigateToPage) {
        this.router.navigateByUrl(response.navigateToPage);
      }
      else if (response.dirty) {
        this._refreshing = true;
        this._previousScrollPosition = window.scrollY || document.getElementsByTagName("html")[0].scrollTop;
        this.router.navigateByUrl(this.router.url);
      }
    }
  }

  @HostListener('document:keypress', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.key === '/') {
      //this.focusMessage()
    }
    if (event.key === '?' && !this._visible) {
      this.toggleChat();
    }
  }

  toggleSpeechSynthesis() {
    this.speechSynthesisOn = !this.speechSynthesisOn;
  }

  toggleNormalConversation() {
    this.normalConversation = !this.normalConversation;
  }

  _addVoicesList = (voices: any[]) => {
    const list = window.document.createElement("div");
    let html =
      '<h2>Available Voices</h2><select id="languages"><option value="">autodetect language</option>';
    voices.forEach(voice => {
      html += `<option value="${voice.lang}" data-name="${voice.name}">${voice.name
        } (${voice.lang})</option>`;
    });
    list.innerHTML = html;
    window.document.body.appendChild(list);
  };

  stopRecording() {
    this.keepRecording = false;
    this.mediaRecorder.stop();
  }

  record(respond: boolean = false) {
    if (respond) {
      this.receiveMessage({
        chatConversationId: this._chatConversationId,
        chatMessages: [...this.chatMessages, {
          content: 'How can I help you manage your ' + this.getCurrentPageName(),
          rawContent: 'How can I help you manage your ' + this.getCurrentPageName(),
          name: this.assistant.name,
          role: this.assistant.name
        }],
        createNewChat: false,
        dirty: false,
        error: false,
        navigateToPage: undefined
      } as GetChatResponseVm);
    }
    this.timeSinceDetected = 0;
    this.lastTimeDetected = performance.now();
    this.keepRecording = this.visible;
    const MIN_DECIBELS = -45;
    if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
      console.log("This browser does not support the API yet");
    }

    let audioPlayer = this.audioPlayerRef.nativeElement as HTMLAudioElement;
    audioPlayer.crossOrigin = "anonymous";
    audioPlayer.oncanplaythrough = e => {
      audioPlayer.play();
    };
    audioPlayer.onended = e => {
      if (this.visible) {
        if (this.recording) {
          this.lastTimeDetected = performance.now();
        }
        else {
          this.record();
        }
      }
    }

    let hasMicrophone = false;
    navigator.mediaDevices.enumerateDevices().then((devices) => {
      devices.forEach((device) => {
        if (device.kind == 'audioinput') {
          hasMicrophone = true;
        }
      });
    }).catch(function (err) {
      console.log(err.name + ": " + err.message);
    }).then(() => {
      if (hasMicrophone) {
        navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
          if (stream.getAudioTracks().length > 0) {
            console.log("recording");
            console.log('asdf1');
            //this.endSilenceDetected = false;
            this.soundDetectedSendToServer = false;
            this.soundWasDetected = false;
            this.mediaRecorder = new MediaRecorder(stream);
            this.mediaRecorder.start(2000);

            let audioChunks: any[] = [];
            this.mediaRecorder.ondataavailable = (event: { data: any; }) => {
              audioChunks.push(event.data);
              const audioBlob = new Blob(audioChunks);
              console.log("dataavailable");
              if (this.soundWasDetected) {
                if (this.timeSinceDetected > 1) {
                  this.soundDetectedSendToServer = true;
                  this.soundWasDetected = false;
                  this.mediaRecorder.stop();
                }
              } else {
                if (this.timeSinceDetected > 10) {
                  this.keepRecording = false;
                  this.timeSinceDetected = 0;
                  this.lastTimeDetected = performance.now();
                  this.soundDetectedSendToServer = false;
                  this.mediaRecorder.stop();
                }
                if (this.mediaRecorder.state == "inactive") {
                  this.mediaRecorder.stop();
                }
              }
            };

            const audioContext = new AudioContext();
            const audioStreamSource = audioContext.createMediaStreamSource(stream);
            const analyser = audioContext.createAnalyser();
            analyser.minDecibels = MIN_DECIBELS;
            audioStreamSource.connect(analyser);

            const bufferLength = analyser.frequencyBinCount;
            const domainData = new Uint8Array(bufferLength);


            const detectSound = () => {

              analyser.getByteFrequencyData(domainData);
              let detected = false;
              for (let i = 0; i < bufferLength; i++) {
                const value = domainData[i];

                if (domainData[i] > 0) {
                  detected = true;
                  break;
                }
              }
              if (detected) {
                this.lastTimeDetected = performance.now();
                this.soundWasDetected = true;
              }
              else {
                this.timeSinceDetected = Math.floor((performance.now() - this.lastTimeDetected)) / 1000;
              }

              if (this.recording) {
                window.requestAnimationFrame(detectSound);
              }
            };
            window.requestAnimationFrame(detectSound);

            //const detectEndSilence = () => {
            //  analyser.getByteFrequencyData(domainData);

            //  let endSoundDetected = false;
            //  for (let i = bufferLength - 1; i >= 0; i--) {
            //    const value = domainData[i];

            //    if (domainData[i] > 0) {
            //      endSoundDetected = true;
            //    }
            //    if (i < 500) {
            //      if (endSoundDetected == false) {
            //        this.endSilenceDetected = true;
            //      }
            //    }
            //  }

            //  if (this.mediaRecorder.state == "recording") {
            //    window.requestAnimationFrame(detectEndSilence);
            //  }
            //};
            //window.requestAnimationFrame(detectEndSilence);

            this.mediaRecorder.addEventListener("stop", () => {
              console.log("Recording ended.");
              const audioBlob = new Blob(audioChunks);
              if (this.soundDetectedSendToServer) {
                this.soundDetectedSendToServer = false;
                this.sendSpeech(audioBlob);
                if (this.keepRecording) {
                  this.record();
                }
              }
              else {
                console.log("No sound detected. Nothing sent to server");
              }
            });
          }
          else {
            console.log('asdf2');
          }
        }, function (error) {
          console.log(error);
        });
      }
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
          this.greeting = 'An error while transcribing audio.';
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

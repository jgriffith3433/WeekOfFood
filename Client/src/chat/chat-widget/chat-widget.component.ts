import { AfterViewInit, Component, ElementRef, HostListener, Input, OnDestroy, OnInit, ViewChild } from '@angular/core'
import { fadeIn, fadeInOut } from '../animations'
import { NavigationEnd, NavigationStart, Router } from '@angular/router';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, Subject, throwError as _observableThrow, of as _observableOf, Subscription, BehaviorSubject } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { ChatService } from '../providers/chat.service'

interface RecommendedVoices {
  [key: string]: boolean;
}

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

import { ChatMessageVm } from '../models/ChatMessageVm'
import { GetChatResponseQuery } from '../models/GetChatResponseQuery'
import { TokenService } from '../../app/providers/token.service';
import { PicoService } from '../providers/pico.service';
import { GetChatResponseVm } from '../models/GetChatResponseVm';
import { AuthService } from '../../app/providers/auth.service';

//read online that maybe you want to use this instead?
//import { Blob } from 'buffer';


const rand = (max: number) => Math.floor(Math.random() * max)

@Component({
  selector: 'chat-widget',
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.css'],
  animations: [fadeInOut, fadeIn],
})
export class ChatWidgetComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('bottom') bottom: ElementRef;
  @ViewChild('audio') audioPlayerRef: ElementRef;
  @ViewChild('visualizer') visualizerRef: ElementRef;
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
  lastTimeDetected: number = 0;
  timeSinceDetected: number = 0;
  soundDetectedSendToServer: boolean = false;
  soundDetectedLastSecond: boolean = false;
  waitingForEndAudio: boolean = false;
  soundWasDetectedSinceSpeechSent: boolean = false;
  soundWasDetectedSinceLastAvailableData: boolean = false;
  audioBlobsToSend: Blob[] = [];
  canvasCtx: any;
  audioContext: AudioContext;
  recordingBufferSource: AudioBufferSourceNode;
  recordingAudioStreamDestination: MediaStreamAudioDestinationNode;
  audioPlayer: HTMLAudioElement;
  textToSpeechAudioStreamSource: MediaElementAudioSourceNode;
  textToSpeechVisualAnalyser: AnalyserNode;
  textToSpeechVisualAnalyserDataArray: Uint8Array;
  textToSpeechVisualAnalyserBufferLength: number;
  textToSpeechVisualAnalyserAnimationFrameHandle: number | undefined;
  textToSpeechPlaying: boolean = false;
  recordingvolume: number = 1;
  synthvolume: number = 1;
  recordingVisualAnalyserAudioStreamSource: MediaStreamAudioSourceNode;
  recordingVisualAnalyser: AnalyserNode;
  recordingVisualAnalyserDataArray: Uint8Array;
  recordingVisualAnalyserBufferLength: number;
  recordingVisualAnalyserAnimationFrameHandle: number | undefined;
  recordingSoundAnalyserAudioStreamSource: MediaStreamAudioSourceNode;
  recordingSoundAnalyser: AnalyserNode;
  recordingSoundAnalyserDataArray: Uint8Array;
  recordingSoundAnalyserBufferLength: number;
  recordingSoundAnalyserAnimationFrameHandle: number | undefined;
  MIN_DECIBELS = -45;
  badWordAndWakeWordRegex: RegExp = new RegExp("^bumble *bee*|[a@][s\$][s\$]$|[a@][s\$][s\$]h[o0][l1][e3][s\$]?|b[a@][s\$][t\+][a@]rd|b[e3][a@][s\$][t\+][i1][a@]?[l1]([i1][t\+]y)?|b[e3][a@][s\$][t\+][i1][l1][i1][t\+]y|b[e3][s\$][t\+][i1][a@][l1]([i1][t\+]y)?|b[i1][t\+]ch[s\$]?|b[i1][t\+]ch[e3]r[s\$]?|b[i1][t\+]ch[e3][s\$]|b[i1][t\+]ch[i1]ng?|b[l1][o0]wj[o0]b[s\$]?|c[l1][i1][t\+]|^(c|k|ck|q)[o0](c|k|ck|q)[s\$]?$|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[e3]d|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[e3]r|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[i1]ng|(c|k|ck|q)[o0](c|k|ck|q)[s\$]u(c|k|ck|q)[s\$]|^cum[s\$]?$|cumm??[e3]r|cumm?[i1]ngcock|(c|k|ck|q)um[s\$]h[o0][t\+]|(c|k|ck|q)un[i1][l1][i1]ngu[s\$]|(c|k|ck|q)un[i1][l1][l1][i1]ngu[s\$]|(c|k|ck|q)unn[i1][l1][i1]ngu[s\$]|(c|k|ck|q)un[t\+][s\$]?|(c|k|ck|q)un[t\+][l1][i1](c|k|ck|q)|(c|k|ck|q)un[t\+][l1][i1](c|k|ck|q)[e3]r|(c|k|ck|q)un[t\+][l1][i1](c|k|ck|q)[i1]ng|cyb[e3]r(ph|f)u(c|k|ck|q)|d[a@]mn|d[i1]ck|d[i1][l1]d[o0]|d[i1][l1]d[o0][s\$]|d[i1]n(c|k|ck|q)|d[i1]n(c|k|ck|q)[s\$]|[e3]j[a@]cu[l1]|(ph|f)[a@]g[s\$]?|(ph|f)[a@]gg[i1]ng|(ph|f)[a@]gg?[o0][t\+][s\$]?|(ph|f)[a@]gg[s\$]|(ph|f)[e3][l1][l1]?[a@][t\+][i1][o0]|(ph|f)u(c|k|ck|q)|(ph|f)u(c|k|ck|q)[s\$]?|g[a@]ngb[a@]ng[s\$]?|g[a@]ngb[a@]ng[e3]d|g[a@]y|h[o0]m?m[o0]|h[o0]rny|j[a@](c|k|ck|q)\-?[o0](ph|f)(ph|f)?|j[e3]rk\-?[o0](ph|f)(ph|f)?|j[i1][s\$z][s\$z]?m?|[ck][o0]ndum[s\$]?|mast(e|ur)b(8|ait|ate)|n+[i1]+[gq]+[e3]*r+[s\$]*|[o0]rg[a@][s\$][i1]m[s\$]?|[o0]rg[a@][s\$]m[s\$]?|p[e3]nn?[i1][s\$]|p[i1][s\$][s\$]|p[i1][s\$][s\$][o0](ph|f)(ph|f)|p[o0]rn|p[o0]rn[o0][s\$]?|p[o0]rn[o0]gr[a@]phy|pr[i1]ck[s\$]?|pu[s\$][s\$][i1][e3][s\$]|pu[s\$][s\$]y[s\$]?|[s\$][e3]x|[s\$]h[i1][t\+][s\$]?|[s\$][l1]u[t\+][s\$]?|[s\$]mu[t\+][s\$]?|[s\$]punk[s\$]?|[t\+]w[a@][t\+][s\$]?");
  //endSilenceDetected: boolean = false;
  public speechSynthesisOn: boolean = true;
  public normalConversation: boolean = true;
  public authenticated: boolean = false;
  audioSource = '';
  private currentWakeWord: string;
  private keywordSubscription: Subscription;
  private errorSubscription: Subscription;
  private authSubscription: Subscription;

  constructor(
    private chatService: ChatService,
    private router: Router,
    private tokenService: TokenService,
    private picoService: PicoService,
    private authService: AuthService,
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

  public get recordingPaused() {
    return this.mediaRecorder && this.mediaRecorder.state == "paused";
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

  ensureAudioContextCreated() {
    if (!this.audioContext) {
      this.audioContext = new AudioContext(this.audioContextOptions);
      this.recordingBufferSource = this.audioContext.createBufferSource();
      this.recordingAudioStreamDestination = this.audioContext.createMediaStreamDestination();

      this.recordingVisualAnalyserAudioStreamSource = this.audioContext.createMediaStreamSource(this.recordingAudioStreamDestination.stream);
      this.recordingVisualAnalyser = this.audioContext.createAnalyser();
      this.recordingVisualAnalyser.fftSize = 2048;
      this.recordingVisualAnalyserBufferLength = this.recordingVisualAnalyser.frequencyBinCount;
      this.recordingVisualAnalyserDataArray = new Uint8Array(this.recordingVisualAnalyserBufferLength);
      this.recordingVisualAnalyserAudioStreamSource.connect(this.recordingVisualAnalyser);


      this.recordingSoundAnalyserAudioStreamSource = this.audioContext.createMediaStreamSource(this.recordingAudioStreamDestination.stream);
      this.recordingSoundAnalyser = this.audioContext.createAnalyser();
      this.recordingSoundAnalyser.fftSize = 2048;
      this.recordingSoundAnalyser.minDecibels = this.MIN_DECIBELS;
      this.recordingSoundAnalyserBufferLength = this.recordingSoundAnalyser.frequencyBinCount;
      this.recordingSoundAnalyserDataArray = new Uint8Array(this.recordingSoundAnalyserBufferLength);
      this.recordingSoundAnalyserAudioStreamSource.connect(this.recordingSoundAnalyser);





      this.textToSpeechAudioStreamSource = this.audioContext.createMediaElementSource(this.audioPlayer);
      this.textToSpeechVisualAnalyser = this.audioContext.createAnalyser();
      this.textToSpeechVisualAnalyser.fftSize = 2048;
      this.textToSpeechVisualAnalyserBufferLength = this.textToSpeechVisualAnalyser.frequencyBinCount;
      this.textToSpeechVisualAnalyserDataArray = new Uint8Array(this.textToSpeechVisualAnalyserBufferLength);
      this.textToSpeechAudioStreamSource.connect(this.textToSpeechVisualAnalyser);

      //this.textToSpeechDestination = this.audioContext.createMediaStreamDestination();
      //this.textToSpeechAudioGainNode = this.audioContext.createGain();
      //this.textToSpeechAudioStreamSource.connect(this.textToSpeechAudioGainNode);
      //this.textToSpeechAudioGainNode.connect(this.textToSpeechDestination);

      //let mediaStream = this.audioContext.createMediaStreamSource(this.textToSpeechDestination.stream);
      //mediaStream.connect(this.audioContext.destination);

      this.source2 = this.textToSpeechAudioStreamSource;
      //this.source2 = this.audioContext.createMediaElementSource(this.audioPlayer);
      this.textToSpeechDestination = this.audioContext.createMediaStreamDestination();
      this.textToSpeechAudioGainNode = this.audioContext.createGain();
      this.source2.connect(this.textToSpeechAudioGainNode);
      this.textToSpeechAudioGainNode.connect(this.textToSpeechDestination);
      let mediaStream = this.audioContext.createMediaStreamSource(this.textToSpeechDestination.stream);
      mediaStream.connect(this.audioContext.destination);





    }
  }
  recordingAudioGainNode: GainNode;
  textToSpeechAudioGainNode: GainNode;
  //public focusMessage() {
  //  this.focus.next(true)
  //}

  ngAfterViewInit() {
    this.audioPlayer = this.audioPlayerRef.nativeElement as HTMLAudioElement;
    this.canvasCtx = this.visualizerRef.nativeElement.getContext("2d");

    this.audioPlayer.crossOrigin = "anonymous";
    this.audioPlayer.oncanplaythrough = e => {
      this.textToSpeechPlaying = true;
      this.audioPlayer.play();
      if (this.textToSpeechVisualAnalyserAnimationFrameHandle) {
        window.cancelAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandle);
        this.textToSpeechVisualAnalyserAnimationFrameHandle = undefined;
      }
      this.textToSpeechVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandler);
    };
    this.audioPlayer.onended = e => {
      this.textToSpeechPlaying = false;
      if (this.visible) {
        if (this.recording) {
          this.lastTimeDetected = performance.now();
        }
        else {
          this.record();
        }
      }
    }
    this.audioPlayer.onpause = e => {
      this.textToSpeechPlaying = false;
      if (this.recording) {
        this.lastTimeDetected = performance.now();
      }
      else {
        this.record();
      }
    }
  }
  mediaStreamConstraints: MediaStreamConstraints;
  audioContextOptions: AudioContextOptions;

  async ngOnInit() {
    navigator.mediaDevices.getUserMedia({ audio: true }).then(checkStream => {
      if (checkStream.getAudioTracks().length > 0) {
        let track = checkStream.getAudioTracks()[0];
        let capabilities = track.getCapabilities();
        console.log('Track Capabilities:');
        console.log(track.getCapabilities());

        this.mediaStreamConstraints = {
          audio: {
            noiseSuppression: false,
            suppressLocalAudioPlayback: false,
            advanced: [{
              noiseSuppression: false,
              sampleRate: capabilities.sampleRate?.max,
              sampleSize: capabilities.sampleSize?.max,
              suppressLocalAudioPlayback: false,
            }]
          }
        } as MediaStreamConstraints;

        this.audioContextOptions = {
          audio: {
            channelCount: 1,
            sampleRate: capabilities.sampleRate?.max,
            sampleSize: capabilities.sampleSize?.max,
            volume: 1,
          },
          noiseSuppression: false,
          sampleRate: capabilities.sampleRate?.max
        } as AudioContextOptions;
      }
    });

    window.onresize = () => {
      //this.visualizerRef.nativeElement.width = mainSection.offsetWidth;
    }

    this.authenticated = this.tokenService.IsAuthenticated;
    this.authSubscription = this.authService.authListener.subscribe(authenticated => {
      this.authenticated = authenticated;
      this.scrollToBottom();
      this.sendMessages();
    });
    this.keywordSubscription = this.picoService.keywordDetectionListener.subscribe(porcupineSubscription => {
      this.currentWakeWord = porcupineSubscription.label;
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
    this.authSubscription.unsubscribe();
    this.keywordSubscription.unsubscribe();
    this.errorSubscription.unsubscribe();
    if (this.textToSpeechVisualAnalyserAnimationFrameHandle) {
      window.cancelAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandle);
      this.textToSpeechVisualAnalyserAnimationFrameHandle = undefined;
    }

    if (this.recordingVisualAnalyserAnimationFrameHandle) {
      window.cancelAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandle);
      this.recordingVisualAnalyserAnimationFrameHandle = undefined;
    }

    if (this.recordingSoundAnalyserAnimationFrameHandle) {
      window.cancelAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandle);
      this.recordingSoundAnalyserAnimationFrameHandle = undefined;
    }
  }

  public userToggleChat() {
    this.ensureAudioContextCreated();
    this.toggleChat();
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

  public userSendMessage({ message }: any) {
    this.ensureAudioContextCreated();
    this.sendMessage(message);
  }

  public sendMessage(message: string) {
    if (message.trim() === '') {
      return;
    }

    this.addMessage({
      content: message,
      rawContent: message,
      from: this.user.name,
      to: this.assistant.name,
      received: false
    } as ChatMessageVm);

    this.scrollToBottom();
    this.sendMessages();
  }

  sendMessages() {
    let unsentMessages = false;
    this.chatMessages.forEach(c => {
      if (c.from == this.user.name && !c.received) {
        unsentMessages = true;
      }
    });
    if (!unsentMessages) {
      return;
    }
    let query: GetChatResponseQuery = {
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
                    from: this.system.name,
                    to: this.user.name,
                    received: true
                  } as ChatMessageVm);
                }
                console.error(error.errors[e]);
              }
            }
            else if (error.message) {
              this.addMessage({
                content: error.message,
                rawContent: error.message,
                from: this.system.name,
                to: this.user.name,
                received: true
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
      if (this.chatMessages[i].from == this.system.name) {
        return this.chatMessages[i].content;
      }
    }
    return this.greeting;
  }

  receiveMessage(response: GetChatResponseVm) {
    let newChatMessages: ChatMessageVm[] = [];
    if (response.chatMessages) {
      //update message received
      for (let i = 0; i < this.chatMessages.length; i++) {
        if (i <= response.chatMessages.length - 1) {
          this.chatMessages[i].received = response.chatMessages[i].received;
        }
      }

      //get new messages
      for (let i = this.chatMessages.length; i < response.chatMessages.length; i++) {
        if (response.chatMessages[i].to == this.user.name) {
          response.chatMessages[i].received = true;
        }
        newChatMessages.push(response.chatMessages[i]);
      }
    }
    if (this.speechSynthesisOn) {
      let textToSpeak = "";
      if (newChatMessages.length > 0) {
        let mostRecentMessage = newChatMessages[newChatMessages.length - 1];
        if (mostRecentMessage.content && mostRecentMessage.from == this.assistant.name) {
          textToSpeak = mostRecentMessage.content;
        }
      }

      if (textToSpeak && textToSpeak.indexOf('Rate limit reached') == -1) {
        this.chatService.getChatTextToSpeech(textToSpeak).subscribe(result => {
          this.audioSource = result;
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
    this.sendMessages();
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

  source: MediaStreamAudioSourceNode;
  source2: MediaElementAudioSourceNode;
  textToSpeechDestination: MediaStreamAudioDestinationNode;

  record(respond: boolean = false) {
    this.ensureAudioContextCreated();
    if (respond) {
      this.receiveMessage({
        chatConversationId: this._chatConversationId,
        chatMessages: [...this.chatMessages, {
          content: 'How can I help you manage your ' + this.getCurrentPageName(),
          rawContent: 'How can I help you manage your ' + this.getCurrentPageName(),
          from: this.assistant.name,
          to: this.user.name,
          received: true
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
    if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
      console.log("This browser does not support the API yet");
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

        navigator.mediaDevices.getUserMedia({ audio: true }).then(checkStream => {
          if (checkStream.getAudioTracks().length > 0) {



            navigator.mediaDevices.getUserMedia(this.mediaStreamConstraints).then(stream => {
              if (stream.getAudioTracks().length > 0) {
                let track = stream.getAudioTracks()[0];
                let constraints = track.getConstraints();
                console.log('Track Constraints:');
                console.log(constraints);

                let mediaTrackSettings = track.getSettings();
                console.log('Track Settings:');
                console.log(mediaTrackSettings);

                this.source = this.audioContext.createMediaStreamSource(stream);
                this.recordingAudioGainNode = this.audioContext.createGain();
                this.source.connect(this.recordingAudioGainNode);
                this.recordingAudioGainNode.connect(this.recordingAudioStreamDestination);
                let mediaStreamSourceNode = this.audioContext.createMediaStreamSource(this.recordingAudioStreamDestination.stream);
                //mediaStream.connect(this.audioContext.destination);


                if (this.recordingAudioStreamDestination.stream.getAudioTracks().length > 0) {
                  let recordingTrack = this.recordingAudioStreamDestination.stream.getAudioTracks()[0];
                  //this.endSilenceDetected = false;
                  this.soundDetectedSendToServer = false;
                  this.soundWasDetectedSinceSpeechSent = false;
                  this.soundWasDetectedSinceLastAvailableData = false;


                  let mediaRecorderOptions = {
                    mimeType: 'audio/ogg',
                    audioBitsPerSecond: 128000
                  } as MediaRecorderOptions;

                  //this.mediaRecorder = new MediaRecorder(this.recordingAudioStreamDestination.stream, mediaRecorderOptions);
                  this.mediaRecorder = new MediaRecorder(this.recordingAudioStreamDestination.stream);
                  //this.mediaRecorder.(16 * 44100);
                  //this.mediaRecorder.setAudioSamplingRate(44100);

                  this.mediaRecorder.ondataavailable = (event: { data: Blob; }) => {
                    // capture beginning of file
                    if (this.audioBlobsToSend.length == 0) {
                      console.log("beginning audio blob");
                      this.audioBlobsToSend.push(event.data);
                    }
                    else if (this.waitingForEndAudio == true) {
                      // capture end of file
                      console.log("end audio blob");
                      this.audioBlobsToSend.push(event.data);
                    }
                    else {
                      // capture middle and trim if no sound is detected
                      if (this.soundWasDetectedSinceSpeechSent) {
                        if (this.soundWasDetectedSinceLastAvailableData || this.soundDetectedLastSecond) {
                          console.log("middle sound captured");
                          this.audioBlobsToSend.push(event.data);
                          this.soundWasDetectedSinceLastAvailableData = false;
                        }
                        if (this.timeSinceDetected > 2) {
                          this.waitingForEndAudio = true;
                          //this.mediaRecorder.requestData();
                          this.mediaRecorder.stop();
                          this.soundDetectedSendToServer = true;
                          this.soundWasDetectedSinceSpeechSent = false;
                        }
                      } else {
                        console.log("no middle sound detected");
                        this.audioBlobsToSend.push(event.data);
                        if (this.textToSpeechPlaying) {
                          this.lastTimeDetected = performance.now();
                        }
                        else {
                          if (this.timeSinceDetected > 10) {
                            this.mediaRecorder.stop();
                            this.timeSinceDetected = 0;
                            this.lastTimeDetected = performance.now();
                          }
                        }
                        //if (this.mediaRecorder.state == "inactive") {
                        //  this.mediaRecorder.stop();
                        //}
                      }
                    }
                  };


                  this.mediaRecorder.onstart = e => {
                    console.log("recording");
                    if (this.recordingVisualAnalyserAnimationFrameHandle) {
                      window.cancelAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandle);
                      this.recordingVisualAnalyserAnimationFrameHandle = undefined;
                    }
                    this.recordingVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandler);

                    if (this.recordingSoundAnalyserAnimationFrameHandle) {
                      window.cancelAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandle);
                      this.recordingSoundAnalyserAnimationFrameHandle = undefined;
                    }
                    this.recordingSoundAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandler);
                  }

                  this.mediaRecorder.onstop = e => {
                    this.waitingForEndAudio = false;
                    console.log("Recording ended.");
                    if (this.soundDetectedSendToServer) {
                      console.log("Sound detected. Sending to server");
                      this.soundDetectedSendToServer = false;
                      this.sendSpeech(true);
                      if (this.keepRecording) {
                        this.mediaRecorder.start(1000);
                      }
                    }
                    else {
                      this.audioBlobsToSend = [];
                      console.log("No sound detected. Nothing sent to server");
                    }
                  };

                  this.mediaRecorder.start(1000);
                }
              }
            }, function (error) {
              console.log(error);
            });
          }
        });
      }
    });
  }

  textToSpeechVisualAnalyserAnimationFrameHandler = () => {
    this.visualizeSound(this.textToSpeechVisualAnalyser, this.textToSpeechVisualAnalyserDataArray, this.textToSpeechVisualAnalyserBufferLength);
    if (this.recordingAudioGainNode) {
      this.unduckRecordingAudio(this.textToSpeechVisualAnalyser, this.textToSpeechVisualAnalyserDataArray, this.textToSpeechVisualAnalyserBufferLength);
    }
    this.textToSpeechVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.textToSpeechVisualAnalyserAnimationFrameHandler);
  }

  unduckRecordingAudio(visualAnalyser: AnalyserNode, visualAnalyserDataArray: Uint8Array, visualAnalyserBufferLength: number) {
    visualAnalyser.getByteTimeDomainData(visualAnalyserDataArray);

    let sliceWidth = visualAnalyserBufferLength / 100;
    let x = 0;
    let heighestV = 0;
    for (let i = 0; i < visualAnalyserBufferLength; i++) {
      let v = visualAnalyserDataArray[i] / 128.0;
      if (v > heighestV) {
        heighestV = v;
      }
      x += sliceWidth;
    }
    let audioGain = heighestV;
    audioGain = audioGain - 1;
    if (audioGain > 1) {
      audioGain = 1;
    }
    if (audioGain < 0) {
      audioGain = 0;
    }
    if (1 + (audioGain * 2.5) > this.recordingAudioGainNode.gain.value) {
      this.recordingAudioGainNode.gain.value += .1;
      if (this.recordingAudioGainNode.gain.value > 3) {
        this.recordingAudioGainNode.gain.value = 3;
      }
    }
    else if (1 + (audioGain * 2.5) < this.recordingAudioGainNode.gain.value) {
      this.recordingAudioGainNode.gain.value -= .005;
      if (this.recordingAudioGainNode.gain.value < 1) {
        this.recordingAudioGainNode.gain.value = 1;
      }
    }
    this.recordingvolume = this.recordingAudioGainNode.gain.value;
  }

  recordingVisualAnalyserAnimationFrameHandler = () => {
    this.visualizeSound(this.recordingVisualAnalyser, this.recordingVisualAnalyserDataArray, this.recordingVisualAnalyserBufferLength);
    this.duckTextToSpeechAudio(this.recordingVisualAnalyser, this.recordingVisualAnalyserDataArray, this.recordingVisualAnalyserBufferLength);
    this.recordingVisualAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingVisualAnalyserAnimationFrameHandler);
  }

  duckTextToSpeechAudio(visualAnalyser: AnalyserNode, visualAnalyserDataArray: Uint8Array, visualAnalyserBufferLength: number) {
    visualAnalyser.getByteTimeDomainData(visualAnalyserDataArray);

    let sliceWidth = visualAnalyserBufferLength / 100;
    let x = 0;
    let heighestV = 0;
    for (let i = 0; i < visualAnalyserBufferLength; i++) {
      let v = visualAnalyserDataArray[i] / 128.0;
      if (v > heighestV) {
        heighestV = v;
      }
      x += sliceWidth;
    }
    let audioGain = heighestV;
    audioGain = audioGain - 1;
    if (audioGain > 1) {
      audioGain = 1;
    }
    if (audioGain < 0) {
      audioGain = 0;
    }
    if (1 - (audioGain * 2.5) > this.textToSpeechAudioGainNode.gain.value) {
      this.textToSpeechAudioGainNode.gain.value += .0005;
      if (this.textToSpeechAudioGainNode.gain.value > 1) {
        this.textToSpeechAudioGainNode.gain.value = 1;
      }
    }
    else if (1 - (audioGain * 2.5) < this.textToSpeechAudioGainNode.gain.value) {
      this.textToSpeechAudioGainNode.gain.value -= .05;
      if (this.textToSpeechAudioGainNode.gain.value < .3) {
        this.textToSpeechAudioGainNode.gain.value = .3;
      }
    }
    this.synthvolume = this.textToSpeechAudioGainNode.gain.value;
  }

  visualizeSound(visualAnalyser: AnalyserNode, visualAnalyserDataArray: Uint8Array, visualAnalyserBufferLength: number) {
    const WIDTH = this.visualizerRef.nativeElement.width
    const HEIGHT = this.visualizerRef.nativeElement.height;

    visualAnalyser.getByteTimeDomainData(visualAnalyserDataArray);

    this.canvasCtx.fillStyle = 'rgb(200, 200, 200)';
    this.canvasCtx.fillRect(0, 0, WIDTH, HEIGHT);
    this.canvasCtx.lineWidth = 2;
    this.canvasCtx.strokeStyle = 'rgb(0, 0, 0)';
    this.canvasCtx.beginPath();

    let sliceWidth = WIDTH * 1.0 / visualAnalyserBufferLength;
    let x = 0;

    for (let i = 0; i < visualAnalyserBufferLength; i++) {

      let v = visualAnalyserDataArray[i] / 128.0;
      let y = v * HEIGHT / 2;

      if (i === 0) {
        this.canvasCtx.moveTo(x, y);
      } else {
        this.canvasCtx.lineTo(x, y);
      }

      x += sliceWidth;
    }

    this.canvasCtx.lineTo(this.visualizerRef.nativeElement.width, this.visualizerRef.nativeElement.height / 2);
    this.canvasCtx.stroke();
  }

  recordingSoundAnalyserAnimationFrameHandler = () => {
    this.detectSound(this.recordingSoundAnalyser, this.recordingSoundAnalyserDataArray, this.recordingSoundAnalyserBufferLength);
    this.recordingSoundAnalyserAnimationFrameHandle = window.requestAnimationFrame(this.recordingSoundAnalyserAnimationFrameHandler);
  }

  detectSound(soundAnalyser: AnalyserNode, soundDataArray: Uint8Array, soundAnalyserBufferLength: number) {
    soundAnalyser.getByteFrequencyData(soundDataArray);
    let detected = false;
    for (let i = 0; i < soundAnalyserBufferLength; i++) {
      const value = soundDataArray[i];

      if (value > 0) {
        detected = true;
        break;
      }
    }
    if (detected) {
      this.timeSinceDetected = 0;
      this.soundDetectedLastSecond = true;
      this.lastTimeDetected = performance.now();
      this.soundWasDetectedSinceLastAvailableData = true;
      this.soundWasDetectedSinceSpeechSent = true;
    }
    else {
      this.timeSinceDetected = Math.floor((performance.now() - this.lastTimeDetected)) / 1000;
      this.soundDetectedLastSecond = this.timeSinceDetected <= 1;
    }
  }

  sendSpeech(compress = false) {
    if (this.textToSpeechPlaying) {
      this.audioPlayer.pause();
    }
    var blobToSend = new Blob(this.audioBlobsToSend, { type: 'audio/webm' });
    this.audioBlobsToSend = [];
    blobToSend.arrayBuffer().then(arrayBuffer => {
      if (this.audioContextOptions.sampleRate) {
        const audioContext = new AudioContext({
          sampleRate: this.audioContextOptions.sampleRate
        });
        audioContext.decodeAudioData(arrayBuffer).then(audioBuffer => {
          //const newAudioBuffer = audioContext.createBuffer(
          //  audioBuffer.numberOfChannels,
          //  audioBuffer.length,
          //  audioBuffer.sampleRate
          //);

          //// Copy the modified audio data to the new audio buffer
          //for (let channel = 0; channel < audioBuffer.numberOfChannels; channel++) {
          //  const channelData = newAudioBuffer.getChannelData(channel);
          //  const originalChannelData = audioBuffer.getChannelData(channel);
          //  channelData.set(originalChannelData);
          //}


          //let threshold = this.calculateSilenceThresholdRMS(audioBuffer, 4);
          //if (threshold < .1) {
          //  threshold = .1;
          //}
          //console.log('threshold: ' + threshold);
          let threshold = .1;
          const frameSize = 2048;
          const hopSize = 1024;
          const trimmedBuffer = this.trimSilenceWithRoom(audioBuffer, threshold, hopSize, frameSize, audioContext);
          console.log('duration after trim: ' + trimmedBuffer.duration);
          if (this.audioContextOptions.sampleRate) {
            var offlineAudioCtx = new OfflineAudioContext({
              numberOfChannels: 1,
              length: this.audioContextOptions.sampleRate * trimmedBuffer.duration,
              sampleRate: this.audioContextOptions.sampleRate,
            });

            let soundSource = offlineAudioCtx.createBufferSource();
            soundSource.buffer = trimmedBuffer;

            if (compress) {
              // Create Compressor Node
              var compressor = offlineAudioCtx.createDynamicsCompressor();
              compressor.threshold.setValueAtTime(-20, offlineAudioCtx.currentTime);
              compressor.knee.setValueAtTime(0, offlineAudioCtx.currentTime);
              compressor.ratio.setValueAtTime(5, offlineAudioCtx.currentTime);
              compressor.attack.setValueAtTime(.05, offlineAudioCtx.currentTime);
              compressor.release.setValueAtTime(.25, offlineAudioCtx.currentTime);

              // Connect nodes to destination
              soundSource.connect(compressor);
              compressor.connect(offlineAudioCtx.destination);
            }
            else {
              soundSource.connect(offlineAudioCtx.destination);
            }

            offlineAudioCtx.startRendering().then((renderedBuffer) => {
              var duration = renderedBuffer.duration;
              console.log("rendered duration: " + duration);
              let rate = renderedBuffer.sampleRate;
              let offset = 0;
              let waveBlob = this.bufferToWave(renderedBuffer, offlineAudioCtx.length)
              //var waveFile = URL.createObjectURL(waveBlob);
              if (duration >= .25) {
                this.sendSpeechToServer(waveBlob);
              }

              soundSource.loop = false;
            }).catch((error) => {
              console.error(error);
            });

            soundSource.start(0);
          }
        });
      }
    });
  }

  sendSpeechToServer(trimmedBlob: Blob) {
    let lastMessage:string | undefined = undefined;
    for (let i = this.chatMessages.length - 1; i >= 0; i--) {
      if (this.chatMessages[i].from == this.user.name) {
        lastMessage = this.chatMessages[i].content;
        break;
      }
    }
    this.chatService.getChatSpeechToText(trimmedBlob, lastMessage).subscribe(
      result => {
        if (result.text) {
          //remove bad words and wake words
          let sentence = "";
          if (result.text.indexOf(" ") != - 1) {
            for (let word of result.text.split(" ")) {
              if (this.badWordAndWakeWordRegex.test(word.toLowerCase())) {
                word = "";
              }
              else {
                sentence += word + " ";
              }
            }
          }
          else {
            if (!this.badWordAndWakeWordRegex.test(result.text)) {
              sentence = result.text;
            }
          }
          let speechText = sentence.trim();
          if (speechText.toLowerCase() == 'stop') {
            speechText = "";
          }
          if (speechText.length == 1 && !RegExp(/^`p{L}/, 'u').test(speechText)) {
            speechText = "";
          }
          if (speechText) {
            this.sendMessage(speechText);
          }
        }
      },
      error => {
        if (error.ok === false) {
          switch (error.status) {
            case 400:
              this.addMessage({
                content: 'An error while transcribing audio.',
                rawContent: 'An error while transcribing audio.',
                from: this.system.name,
                to: this.user.name,
                received: false
              } as ChatMessageVm);
              break;
            case 401:
              this.addMessage({
                content: 'You must be logged in.',
                rawContent: 'You must be logged in.',
                from: this.system.name,
                to: this.user.name,
                received: false
              } as ChatMessageVm);
              break;
            default:
              this.addMessage({
                content: 'An error while transcribing audio.',
                rawContent: 'An error while transcribing audio.',
                from: this.system.name,
                to: this.user.name,
                received: false
              } as ChatMessageVm);
              break;
          }
        }
      }
    );
  }


  audioBufferToBuffer(audioBuffer: AudioBuffer): ArrayBuffer {
    const numberOfChannels = audioBuffer.numberOfChannels;
    const length = audioBuffer.length;
    const sampleRate = audioBuffer.sampleRate;

    const buffer = new ArrayBuffer(length * numberOfChannels * 2); // 2 bytes per sample (16-bit audio)
    const view = new DataView(buffer);

    for (let channel = 0; channel < numberOfChannels; channel++) {
      const channelData = audioBuffer.getChannelData(channel);
      const offset = channel * length;

      for (let i = 0; i < length; i++) {
        const sample = channelData[i];
        const sampleValue = Math.max(-1, Math.min(1, sample));
        const sample16 = sampleValue < 0 ? sampleValue * 0x8000 : sampleValue * 0x7FFF;

        view.setInt16((offset + i) * 2, sample16, true); // true for little-endian
      }
    }

    return buffer;
  }

  bufferToWave(abuffer: AudioBuffer, len: number) {
    var numOfChan = abuffer.numberOfChannels,
      length = len * numOfChan * 2 + 44,
      buffer = new ArrayBuffer(length),
      view = new DataView(buffer),
      channels = [], i, sample,
      offset = 0,
      pos = 0;

    // write WAVE header
    setUint32(0x46464952);                         // "RIFF"
    setUint32(length - 8);                         // file length - 8
    setUint32(0x45564157);                         // "WAVE"

    setUint32(0x20746d66);                         // "fmt " chunk
    setUint32(16);                                 // length = 16
    setUint16(1);                                  // PCM (uncompressed)
    setUint16(numOfChan);
    setUint32(abuffer.sampleRate);
    setUint32(abuffer.sampleRate * 2 * numOfChan); // avg. bytes/sec
    setUint16(numOfChan * 2);                      // block-align
    setUint16(16);                                 // 16-bit (hardcoded in this demo)

    setUint32(0x61746164);                         // "data" - chunk
    setUint32(length - pos - 4);                   // chunk length

    // write interleaved data
    for (i = 0; i < abuffer.numberOfChannels; i++)
      channels.push(abuffer.getChannelData(i));

    while (pos < length) {
      for (i = 0; i < numOfChan; i++) {             // interleave channels
        sample = Math.max(-1, Math.min(1, channels[i][offset])); // clamp
        sample = (0.5 + sample < 0 ? sample * 32768 : sample * 32767) | 0; // scale to 16-bit signed int
        view.setInt16(pos, sample, true);          // write 16-bit sample
        pos += 2;
      }
      offset++                                     // next source sample
    }

    // create Blob
    return new Blob([buffer], { type: "audio/wav" });

    function setUint16(data: number) {
      view.setUint16(pos, data, true);
      pos += 2;
    }

    function setUint32(data: number) {
      view.setUint32(pos, data, true);
      pos += 4;
    }
  }

  MIN_SILENCE_DURATION = 0.5;

  trimSilence(audioBuffer: AudioBuffer, threshold: number, hopSize: number, frameSize: number, audioContext: AudioContext) {
    const channelData = audioBuffer.getChannelData(0); // Assuming mono audio
    const sampleRate = audioBuffer.sampleRate;

    const numFrames = Math.ceil(channelData.length / hopSize);
    const nonSilentFrames = [];

    let lastFrameWasSilent = true;
    // Analyze the audio data in frames and detect non silent sections
    for (let i = 0; i < numFrames; i++) {
      const startSample = i * hopSize;
      const endSample = Math.min(startSample + frameSize, channelData.length);

      let isSilent = true;

      for (let j = startSample; j < endSample; j++) {
        if (Math.abs(channelData[j]) > threshold) { // Adjust the threshold value as needed
          isSilent = false;
          break;
        }
      }

      //make sure not to cut off any audio !lastFrameWasSilent
      if (!isSilent || !lastFrameWasSilent) {
        nonSilentFrames.push({
          start: startSample / sampleRate,
          end: endSample / sampleRate
        });
      }
      lastFrameWasSilent = isSilent;
    }

    // Merge consecutive non silent sections that are shorter than the minimum duration
    const mergedSections = [];
    let currentSection = null;

    for (const nonSilentSection of nonSilentFrames) {
      if (!currentSection) {
        currentSection = nonSilentSection;
      } else if (nonSilentSection.start - currentSection.end <= this.MIN_SILENCE_DURATION) {
        currentSection.end = nonSilentSection.end;
      } else {
        mergedSections.push(currentSection);
        currentSection = nonSilentSection;
      }
    }

    if (currentSection) {
      mergedSections.push(currentSection);
    }

    // Create a new audio buffer with the non-silent audio data
    const newNonSilentDuration = mergedSections.reduce((duration, section) => {
      return duration + (section.end - section.start);
    }, 0);

    const newBuffer = audioContext.createBuffer(1, Math.floor(newNonSilentDuration * sampleRate), sampleRate);

    let newBufferOffset = 0;

    for (const section of mergedSections) {
      const startSample = Math.floor(section.start * sampleRate);
      const endSample = Math.ceil(section.end * sampleRate);
      const sectionDuration = section.end - section.start;

      for (let channel = 0; channel < newBuffer.numberOfChannels; channel++) {
        const channelData = audioBuffer.getChannelData(channel);
        const newChannelData = newBuffer.getChannelData(channel);

        for (let i = startSample; i < endSample; i++) {
          newChannelData[newBufferOffset + i - startSample] = channelData[i];
        }
      }

      newBufferOffset += Math.floor(sectionDuration * sampleRate);
    }

    return newBuffer;
  }


  trimSilenceWithRoom(audioBuffer: AudioBuffer, threshold: number, hopSize: number, frameSize: number, audioContext: AudioContext) {
    const channelData = audioBuffer.getChannelData(0); // Assuming mono audio
    const sampleRate = audioBuffer.sampleRate;

    const numFrames = Math.ceil(channelData.length / hopSize);
    const nonSilentFrames = [];

    let lastFrameWasSilent = true;
    let previousFrame: any;
    // Analyze the audio data in frames and detect non silent sections
    for (let i = 0; i < numFrames; i++) {
      const startSample = i * hopSize;
      const endSample = Math.min(startSample + frameSize, channelData.length);

      let isSilent = true;

      for (let j = startSample; j < endSample; j++) {
        if (Math.abs(channelData[j]) > threshold) { // Adjust the threshold value as needed
          isSilent = false;
          break;
        }
      }

      let frame = {
        start: startSample / sampleRate,
        end: endSample / sampleRate
      };
      if (!isSilent) {
        //include audio just before
        if (nonSilentFrames.length == 0) {
          if (previousFrame) {
            nonSilentFrames.push(previousFrame);
          }
        }
        nonSilentFrames.push(frame);
      }
      else {
        //include audio just after
        if (!lastFrameWasSilent) {
          nonSilentFrames.push(frame);
        }
      }
      lastFrameWasSilent = isSilent;
      previousFrame = frame;
    }

    // Merge consecutive non silent sections that are shorter than the minimum duration
    const mergedSections = [];
    let currentSection = null;

    for (const nonSilentSection of nonSilentFrames) {
      if (!currentSection) {
        currentSection = nonSilentSection;
      } else if (nonSilentSection.start - currentSection.end <= this.MIN_SILENCE_DURATION) {
        currentSection.end = nonSilentSection.end;
      } else {
        mergedSections.push(currentSection);
        currentSection = nonSilentSection;
      }
    }

    if (currentSection) {
      mergedSections.push(currentSection);
    }

    // Create a new audio buffer with the non-silent audio data
    const newNonSilentDuration = mergedSections.reduce((duration, section) => {
      return duration + (section.end - section.start);
    }, 0);

    const newBuffer = audioContext.createBuffer(1, Math.floor(newNonSilentDuration * sampleRate), sampleRate);

    let newBufferOffset = 0;

    for (const section of mergedSections) {
      const startSample = Math.floor(section.start * sampleRate);
      const endSample = Math.ceil(section.end * sampleRate);
      const sectionDuration = section.end - section.start;

      for (let channel = 0; channel < newBuffer.numberOfChannels; channel++) {
        const channelData = audioBuffer.getChannelData(channel);
        const newChannelData = newBuffer.getChannelData(channel);

        for (let i = startSample; i < endSample; i++) {
          newChannelData[newBufferOffset + i - startSample] = channelData[i];
        }
      }

      newBufferOffset += Math.floor(sectionDuration * sampleRate);
    }

    return newBuffer;
  }


  calculateSilenceThreshold(audioBuffer: AudioBuffer, adjustmentFactor: number) {
    const channelData = audioBuffer.getChannelData(0);

    let sum = 0;
    for (let i = 0; i < channelData.length; i++) {
      sum += Math.abs(channelData[i]);
    }

    const averageAmplitude = sum / channelData.length;
    const threshold = averageAmplitude * adjustmentFactor;
    return threshold;
  }

  calculateSilenceThresholdRMS(audioBuffer: AudioBuffer, adjustmentFactor: number) {
    const channelData = audioBuffer.getChannelData(0);

    let sumOfSquares = 0;
    for (let i = 0; i < channelData.length; i++) {
      sumOfSquares += channelData[i] * channelData[i];
    }

    const meanSquare = sumOfSquares / channelData.length;
    const rmsAmplitude = Math.sqrt(meanSquare);
    const threshold = rmsAmplitude * adjustmentFactor;

    return threshold;
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

import { Component, ElementRef, HostListener, Input, OnInit, ViewChild } from '@angular/core'
import { fadeIn, fadeInOut } from '../animations'
import { NavigationEnd, NavigationStart, Router } from '@angular/router';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, Subject, throwError as _observableThrow, of as _observableOf } from 'rxjs';
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
  greeting: string;
  _previousScrollPosition = 0;
  _chatConversationId: number = -1;
  chatMessages: ChatMessageVm[] = [];
  private http: HttpClient;
  private baseUrl: string;
  protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;
  mediaRecorder: any;
  keepRecording: boolean = false;
  lastTimeDetected: number;
  timeSinceDetected: number;
  soundDetectedSendToServer: boolean = false;
  soundWasDetected: boolean = false;
  //endSilenceDetected: boolean = false;
  public sayCommand: string;
  public recommendedVoices: RecommendedVoices;
  public rates: number[];
  public selectedRate: number;
  public selectedVoiceName: string;
  public selectedVoice: SpeechSynthesisVoice | null;
  public text: string;
  public voices: SpeechSynthesisVoice[];
  public speechSynthesisOn: boolean = true;
  public normalConversation: boolean = true;

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
          setTimeout(() => {
            this._chatConversationId = -1;
            if (event.url.toLowerCase().indexOf('login') == -1) {
              while (this.messages.length > 0) {
                this.messages.pop();
              }
              while (this.chatMessages.length > 0) {
                this.chatMessages.pop();
              }
              this.greeting = 'How can I help you manage your ' + this.getCurrentPageName();
            }
            if (this._botNavigating) {
              this._botNavigating = false;
            }
          }, 500);
        }
      }
    });

    this.voices = [];
    this.rates = [.25, .5, .75, 1, 1.25, 1.5, 1.75, 2];
    this.selectedVoiceName = "Google UK English Female";
    this.selectedVoice = null;
    this.selectedRate = 1;
    // Dirty Dancing for the win!
    this.sayCommand = "";

    // These are "recommended" in so much as that these are the voices that I (Ben)
    // could understand most clearly.
    this.recommendedVoices = Object.create(null);
    this.recommendedVoices["Alex"] = true;
    this.recommendedVoices["Alva"] = true;
    this.recommendedVoices["Damayanti"] = true;
    this.recommendedVoices["Daniel"] = true;
    this.recommendedVoices["Fiona"] = true;
    this.recommendedVoices["Fred"] = true;
    this.recommendedVoices["Karen"] = true;
    this.recommendedVoices["Mei-Jia"] = true;
    this.recommendedVoices["Melina"] = true;
    this.recommendedVoices["Moira"] = true;
    this.recommendedVoices["Rishi"] = true;
    this.recommendedVoices["Samantha"] = true;
    this.recommendedVoices["Tessa"] = true;
    this.recommendedVoices["Veena"] = true;
    this.recommendedVoices["Victoria"] = true;
    this.recommendedVoices["Yuri"] = true;
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


  // I demo the currently-selected voice.
  public demoSelectedVoice(selectList: any): void {
    this.selectedVoiceName = selectList.currentTarget.value;
    for (var v of this.voices) {
      if (v.name == this.selectedVoiceName) {
        this.selectedVoice = v;
        break;
      }
    }
    if (!this.selectedVoice) {

      console.warn("Expected a voice, but none was selected.");
      return;

    }

    var demoText = "Best wishes and warmest regards.";

    this.stopSpeaking();
    this.synthesizeSpeechFromText(this.selectedVoice, this.selectedRate, demoText);

  }

  // I synthesize speech from the current text for the currently-selected voice.
  public speak(): void {

    if (!this.selectedVoice || !this.text) {

      return;

    }

    this.stopSpeaking();
    this.synthesizeSpeechFromText(this.selectedVoice, this.selectedRate, this.text);

  }


  // I stop any current speech synthesis.
  public stopSpeaking(): void {

    if (speechSynthesis.speaking) {

      speechSynthesis.cancel();

    }

  }


  // I update the "say" command that can be used to generate the a sound file from the
  // current speech synthesis configuration.
  public updateSayCommand(): void {

    if (!this.selectedVoice || !this.text) {

      return;

    }

    // With the say command, the rate is the number of words-per-minute. As such, we
    // have to finagle the SpeechSynthesis rate into something roughly equivalent for
    // the terminal-based invocation.
    var sanitizedRate = Math.floor(200 * this.selectedRate);
    var sanitizedText = this.text
      .replace(/[\r\n]/g, " ")
      .replace(/(["'\\\\/])/g, "\\$1")
      ;

    this.sayCommand = `say --voice ${this.selectedVoice.name} --rate ${sanitizedRate} --output-file=demo.aiff "${sanitizedText}"`;

  }

  // ---
  // PRIVATE METHODS.
  // ---

  // I perform the low-level speech synthesis for the given voice, rate, and text.
  private synthesizeSpeechFromText(
    voice: SpeechSynthesisVoice,
    rate: number,
    text: string
  ): void {

    var utterance = new SpeechSynthesisUtterance(text);
    utterance.voice = this.selectedVoice;
    utterance.rate = rate;

    speechSynthesis.speak(utterance);
    function myTimer() {
      speechSynthesis.pause();
      speechSynthesis.resume();
      (window as any).myTimeout = window.setTimeout(myTimer, 3000);
    }
    if ((window as any).myTimeout) {
      window.clearTimeout((window as any).myTimeout);
    }
    (window as any).myTimeout = window.setTimeout(myTimer, 3000);
  }


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

  ngOnInit() {
    this.voices = speechSynthesis.getVoices();
    for (let v of this.voices) {
      if (this.selectedVoiceName == v.name) {
        this.selectedVoice = v;
        break;
      }
    }
    this.updateSayCommand();

    // The voices aren't immediately available (or so it seems). As such, if no
    // voices came back, let's assume they haven't loaded yet and we need to wait for
    // the "voiceschanged" event to fire before we can access them.
    if (!this.voices.length) {

      speechSynthesis.addEventListener(
        "voiceschanged",
        () => {
          this.voices = speechSynthesis.getVoices();
          for (let v of this.voices) {
            if (this.selectedVoiceName == v.name) {
              this.selectedVoice = v;
              break;
            }
          }
          this.updateSayCommand();

        }
      );
    }
    setTimeout(() => this.visible = true, 2000);
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

    if (this.normalConversation) {
      this.chatService.getNormalChatResponse(query).subscribe(
        result => {
          let newChatMessages: ChatMessageVm[] = [];
          if (result.chatMessages) {
            for (let i = this.chatMessages.length; i < result.chatMessages.length; i++) {
              newChatMessages.push(result.chatMessages[i]);
            }
          }
          if (this.speechSynthesisOn) {
            let textToSpeak = "";
            for (let c of newChatMessages) {
              if (c.content && c.role == this.assistant.name) {
                textToSpeak += c.content + '...';
              }
            }
            if (textToSpeak && textToSpeak.indexOf('Rate limit reached') == -1) {
              this.text = textToSpeak;
              this.speak();
            }
          }
          if (result.createNewChat) {
            if (result.error) {
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
                this.router.navigateByUrl(result.navigateToPage || '/');
              }, 2000);
            }
          }
          else {
            this._chatConversationId = result.chatConversationId || -1;
            for (let newChatMessage of newChatMessages) {
              this.addMessage(newChatMessage);
            }
            this.scrollToBottom();

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
            if (this.getCurrentPageName() != 'login') {
              this.addMessage({
                content: 'An error occured.',
                rawContent: 'An error occured.',
                name: this.system.name,
                role: this.system.name
              } as ChatMessageVm);
              this.scrollToBottom();
            }
          }, 500);
        }
      );
    }
    else {
      this.chatService.getChatResponse(query).subscribe(
        result => {
          let newChatMessages: ChatMessageVm[] = [];
          if (result.chatMessages) {
            for (let i = this.chatMessages.length; i < result.chatMessages.length; i++) {
              newChatMessages.push(result.chatMessages[i]);
            }
          }
          if (this.speechSynthesisOn) {
            let textToSpeak = "";
            for (let c of newChatMessages) {
              if (c.content && c.role == this.assistant.name) {
                textToSpeak += c.content + '...';
              }
            }
            if (textToSpeak && textToSpeak.indexOf('Rate limit reached') == -1) {
              this.text = textToSpeak;
              this.speak();
            }
          }
          if (result.createNewChat) {
            if (result.error) {
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
                this.router.navigateByUrl(result.navigateToPage || '/');
              }, 2000);
            }
          }
          else {
            this._chatConversationId = result.chatConversationId || -1;
            for (let newChatMessage of newChatMessages) {
              this.addMessage(newChatMessage);
            }
            this.scrollToBottom();

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
            if (this.getCurrentPageName() != 'login') {
              this.addMessage({
                content: 'An error occured.',
                rawContent: 'An error occured.',
                name: this.system.name,
                role: this.system.name
              } as ChatMessageVm);
              this.scrollToBottom();
            }
          }, 500);
        }
      );
    }
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

  record() {
    console.log("recording");
    this.keepRecording = true;
    const MIN_DECIBELS = -45;

    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
      //this.endSilenceDetected = false;
      this.soundDetectedSendToServer = false;
      this.soundWasDetected = false;
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.start(3000);

      let audioChunks: any[] = [];
      this.mediaRecorder.addEventListener("dataavailable", (event: { data: any; }) => {
        audioChunks.push(event.data);
        const audioBlob = new Blob(audioChunks);
        console.log("dataavailable");
        //console.log("end silence detected: " + this.endSilenceDetected);
        //if (this.soundDetected && this.endSilenceDetected) {
        if (this.soundWasDetected) {
          if (this.timeSinceDetected > 1) {
            this.soundDetectedSendToServer = true;
            this.soundWasDetected = false;
            this.mediaRecorder.stop();
          }
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

        if (this.mediaRecorder.state == "recording") {
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
        const audioBlob = new Blob(audioChunks);
        if (this.soundDetectedSendToServer) {
          this.soundDetectedSendToServer = false;
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

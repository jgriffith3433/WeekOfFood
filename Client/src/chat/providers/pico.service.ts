import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { PicoModels } from '../models/picoModels';
import { TokenService } from '../../app/providers/token.service';
import { PorcupineService } from '@picovoice/porcupine-angular';
import { PorcupineDetection } from '@picovoice/porcupine-web';

@Injectable({
  providedIn: 'root'
})
export class PicoService implements OnDestroy {
  private keywords: Array<string>;
  public autoStart: boolean = false;
  public isLoaded: boolean = false;
  public isListening: boolean = false;
  private isLoadedSubscription: Subscription;
  private isListeningSubscription: Subscription;

  public get keywordDetectionListener(): Subject<PorcupineDetection> {
    return this.porcupineService.keywordDetection$;
  }

  public get errorListener(): Subject<Error | null> {
    return this.porcupineService.error$;
  }

  constructor(private http: HttpClient,
    private tokenService: TokenService,
    private porcupineService: PorcupineService,
  ) {
    this.keywords = PicoModels.porcupineKeywords.map((k: any) => k.label ?? k.builtin);

    this.isLoadedSubscription = this.porcupineService.isLoaded$.subscribe(isLoaded => {
      this.isLoaded = isLoaded;
      console.log('isLoaded: ' + this.isLoaded);
      console.log('autoStart: ' + this.autoStart);
      if (this.autoStart) {
        this.start();
      }
    });
    this.isListeningSubscription = this.porcupineService.isListening$.subscribe(isListening => {
      this.isListening = isListening;
      console.log('isListening: ' + this.isListening);
    });
  }

  public async start() {
    if (this.isListening) {
      await this.porcupineService.stop();
    }
    await this.porcupineService.start();
  }

  public async stop() {
    if (this.isListening) {
      await this.porcupineService.stop();
    }
  }

  public async load() {
    if (this.isLoaded) {
      await this.porcupineService.release();
    }
    try {
      await this.porcupineService.init(
        this.tokenService.getPico() as string,
        // @ts-ignore
        this.keywords[0],
        PicoModels.porcupineModel
      );
    }
    catch (error: any) {
      if (error) {
        console.error(error);
      }
    }
  }

  public unload() {
    this.isLoadedSubscription.unsubscribe();
    this.isListeningSubscription.unsubscribe();
    this.porcupineService.release();
  }

  ngOnDestroy(): void {
    this.unload();
  }
}
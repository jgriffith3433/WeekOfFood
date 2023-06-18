import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
@Injectable({
  providedIn: 'root'
})
export class HowDoIService {
  public promise: Subject<string>;

  constructor(
  ) {
    this.promise = new Subject<string>();
  }

  public send(how: string) {
    this.promise.next(how);
  }
}

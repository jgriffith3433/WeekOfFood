import { Component, OnInit } from '@angular/core';
import { HowDoIService } from '../../../../chat/providers/how-do-I.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  constructor(
    private howDoIService: HowDoIService,
  ) { }

  ngOnInit(): void {

  }

  youMakeANewRecipe(): void {
    this.howDoIService.send("I want you to make a recipe for me.");
  }

  makeANewRecipe(): void {
    this.howDoIService.send("I want to add my own recipe.");
  }

  logARecipe(): void {
    this.howDoIService.send("Log that I ate a meal");
  }

  takeStock(): void {
    this.howDoIService.send("I'm going to take stock");
  }

  placeAnOrder(): void {
    this.howDoIService.send("I want to place an order");
  }
}

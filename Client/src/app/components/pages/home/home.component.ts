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

  makeANewRecipe(): void {
    this.howDoIService.send("Can you help me create a new recipe?");
  }

  orderEverythingForRecipe(): void {
    this.howDoIService.send("I want to order everything I need for a recipe.");
  }

  whatDoIHaveInStock(): void {
    this.howDoIService.send("What do I have in stock", "auto");
  }

  logARecipe(): void {
    this.howDoIService.send("I just ate a meal, can I tell you what I ate so you can update the system?");
  }

  takeStock(): void {
    this.howDoIService.send("I want to update the kitchen inventory, can I list off what I have so you can update the system?");
  }

  placeAnOrder(): void {
    this.howDoIService.send("I want to place an order");
  }
}

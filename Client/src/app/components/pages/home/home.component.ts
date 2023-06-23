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
    this.howDoIService.send("I want you to make a recipe for me with no ingredients. Ask me for the name of the recipe and how many servings it makes then update the recipe with ingredients that you know. I don't want to provide the ingredients.");
  }

  makeANewRecipe(): void {
    this.howDoIService.send("I want to add my own recipe.");
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
    this.howDoIService.send("I want to manage my kitchen inventory, can I tell you what kitchen products I have in my kitchen so you can update the system?");
  }

  placeAnOrder(): void {
    this.howDoIService.send("I want to place an order");
  }
}

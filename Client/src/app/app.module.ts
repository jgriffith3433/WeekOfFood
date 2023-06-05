import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { ModalModule } from 'ngx-bootstrap/modal';
import { AppRoutingModule } from './app-routing.module';
import { ChatModule } from 'src/chat/chat.module';
import { AppComponent } from './app.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { HomeComponent } from './components/pages/home/home.component';
import { LoginComponent } from './components/pages/login/login.component';
import { AddoreditComponent } from './components/pages/addoredit/addoredit.component';
import { AuthGuard } from './providers/auth.guard';
import { ItemsListComponent } from './components/pages/items-list/items-list.component';
import { TodoListsComponent } from './components/pages/todo-lists/todo-lists.component';
import { TodoListComponent } from './components/pages/todo-list/todo-list.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ProductsComponent } from './components/pages/products/products.component';
import { CompletedOrdersComponent } from './components/pages/completed-orders/completed-orders.component';
import { ProductStocksComponent } from './components/pages/product-stocks/product-stocks.component';
import { CalledIngredientsComponent } from './components/pages/called-ingredients/called-ingredients.component';
import { RecipesComponent } from './components/pages/recipes/recipes.component';
import { CookedRecipesComponent } from './components/pages/cooked-recipes/cooked-recipes.component';
import { HttpErrorInterceptor } from './http-error.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    HomeComponent,
    LoginComponent,
    AddoreditComponent,
    ItemsListComponent,
    TodoListsComponent,
    TodoListComponent,
    ProductsComponent,
    CompletedOrdersComponent,
    ProductStocksComponent,
    CalledIngredientsComponent,
    RecipesComponent,
    CookedRecipesComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    HttpClientModule,
    FormsModule,
    CommonModule,
    BrowserAnimationsModule,
    ChatModule,
    ModalModule.forRoot()
  ],
  providers: [
    AuthGuard,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpErrorInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

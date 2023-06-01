import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './providers/auth.guard';
import { AddoreditComponent } from './components/pages/addoredit/addoredit.component';
import { HomeComponent } from './components/pages/home/home.component';
import { ItemsListComponent } from './components/pages/items-list/items-list.component';
import { TodoListsComponent } from './components/pages/todo-lists/todo-lists.component';
import { TodoListComponent } from './components/pages/todo-list/todo-list.component';
import { LoginComponent } from './components/pages/login/login.component';
import { ProductsComponent } from './components/pages/products/products.component';
import { CompletedOrdersComponent } from './components/pages/completed-orders/completed-orders.component';
import { ProductStocksComponent } from './components/pages/product-stocks/product-stocks.component';
import { CalledIngredientsComponent } from './components/pages/called-ingredients/called-ingredients.component';
import { RecipesComponent } from './components/pages/recipes/recipes.component';
import { CookedRecipesComponent } from './components/pages/cooked-recipes/cooked-recipes.component';

const routes: Routes = [
  {
    path: 'items', canActivate: [AuthGuard], children: [
      { path: '', component: ItemsListComponent },
      { path: 'add', component: AddoreditComponent },
      { path: ':id', component: AddoreditComponent },
      { path: ':id/edit', component: AddoreditComponent }
    ]
  },
  {
    path: 'todo', canActivate: [AuthGuard], children: [
      { path: '', component: TodoListsComponent },
      { path: 'add', component: TodoListComponent },
      { path: ':id', component: TodoListComponent },
      { path: ':id/edit', component: TodoListComponent },
    ]
  },
  {
    path: 'products', canActivate: [AuthGuard], children: [
      { path: '', component: ProductsComponent },
    ]
  },
  {
    path: 'completed-orders', canActivate: [AuthGuard], children: [
      { path: '', component: CompletedOrdersComponent },
    ]
  },
  {
    path: 'product-stocks', canActivate: [AuthGuard], children: [
      { path: '', component: ProductStocksComponent },
    ]
  },
  {
    path: 'called-ingredients', canActivate: [AuthGuard], children: [
      { path: '', component: CalledIngredientsComponent },
    ]
  },
  {
    path: 'recipes', canActivate: [AuthGuard], children: [
      { path: '', component: RecipesComponent },
    ]
  },
  {
    path: 'cooked-recipes', canActivate: [AuthGuard], children: [
      { path: '', component: CookedRecipesComponent },
    ]
  },
  { path: 'login', component: LoginComponent },
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { onSameUrlNavigation: 'reload' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }

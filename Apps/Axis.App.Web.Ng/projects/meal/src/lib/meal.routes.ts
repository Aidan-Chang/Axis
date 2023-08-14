import { Routes } from '@angular/router';
import { MealComponent } from './meal.component';

export const mealRoutes: Routes = [{
  path: '',
  children: [{
    path: '',
    component: MealComponent
  }]
}];

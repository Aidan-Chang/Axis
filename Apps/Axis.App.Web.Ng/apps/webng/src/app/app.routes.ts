import { Routes } from '@angular/router';

export const routes: Routes = [{
  path: '',
  children: [
    { path: '', loadChildren: () => import('@axis/project/meal').then(m => m.mealRoutes) }
  ],
}];

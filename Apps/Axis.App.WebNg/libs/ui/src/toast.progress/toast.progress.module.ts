import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ToastProgressComponent } from './toast.progress.component';
import { ToastProgressItemComponent } from './toast.progress.item.component';
import { ToastProgressInterceptor } from './toast.progress.interceptor';

@NgModule({
  declarations: [
    ToastProgressComponent,
    ToastProgressItemComponent,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: ToastProgressInterceptor, multi: true }
  ]
})
export class AxLibUiToastProgressModule { }
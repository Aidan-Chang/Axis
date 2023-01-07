import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError, EMPTY } from 'rxjs';
import { MessageService, Message } from 'primeng/api';
import { ToastProgressService } from './toast.progress.service';

@Injectable({ providedIn: 'root' })
export class ToastProgressInterceptor implements HttpInterceptor {

  constructor(private service: ToastProgressService, private message: MessageService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let clone: HttpRequest<any>;
    if (this.service.progrssing.length > 0) {
      clone = request.clone({ // add x-toast header if progress is running
        headers: request.headers
          .set('x-toast-id', this.service.progrssing[this.service.progrssing.length - 1])
          .set('x-toast-connection-id', this.service.connection.connectionId || '')
      })
    } else {
      clone = request.clone();
    }
    return next.handle(clone).pipe(
      catchError((response: HttpErrorResponse) => {
        if (clone.headers.has('x-toast-id') == false && clone.headers.has('x-ignore-message') == false) {
          this.message.add({
            severity: 'error',
            statusCode: response.error.statusCode || response.status,
            summary: response?.error?.error?.summary || response?.message || 'App.UnknwonErrorThrown',
            detail: response?.error?.error?.details,
          } as Message);
          return EMPTY;
        }
        return throwError(() => response);
      })
    );
  }

}
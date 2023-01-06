import { Inject, Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpResponse, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { AppConfig, MiniProfiler } from '@axis/lib/core';

declare let MiniProfiler: MiniProfiler;

@Injectable()
export class ProfilerInterceptor implements HttpInterceptor {

  constructor(
    @Inject('config') private config: AppConfig) {
  }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      tap((event: HttpEvent<unknown>) => {
        if (event instanceof HttpResponse && event.headers && typeof MiniProfiler !== 'undefined' && this.config.developer.value$?.value)
          this.fetch(event.headers);
      }),
      catchError((event: HttpErrorResponse) => {
        if (event instanceof HttpErrorResponse && event.headers && typeof MiniProfiler !== 'undefined' && this.config.developer.value$?.value)
          this.fetch(event.headers);
        return throwError(() => event);
      })
    );
  }

  private fetch(headers: HttpHeaders): void {
    const ids = headers.getAll('x-miniprofiler-ids');
    ids?.forEach(header => {
      const ids = JSON.parse(header) as string[];
      MiniProfiler.fetchResults(ids);
    });
  }

}

import { Inject, Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpResponse, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { AppConfig, ProfilerManager } from '@axis/lib/core';

declare let MiniProfiler: any;

@Injectable()
export class ProfilerInterceptor implements HttpInterceptor {

  constructor(
    @Inject('config') private config: AppConfig,
    private profiler: ProfilerManager) {
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      tap((event: HttpEvent<any>) => {
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

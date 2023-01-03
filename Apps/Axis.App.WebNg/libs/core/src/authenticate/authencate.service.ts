import { Injectable, Inject, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Subscription, Observable, of, map, catchError } from 'rxjs';
import { AppConfig, ApiResult } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class AuthenticateService implements OnDestroy {

  lastValidateUrl?: string;
  returnUrl = '';
  private readonly subscriptions$: [Subscription?] = [];

  constructor(
    @Inject('config') private config: AppConfig,
    private jwt: JwtHelperService,
    private http: HttpClient,
    private router: Router,) {
  }

  get token(): string | null {
    const token = localStorage.getItem(this.config.token.name);
    if (token) {
      return token;
    } else {
      return null;
    }
  }

  get tokens(): [string, string] | null {
    const tokens = this.jwt.decodeToken<[string, string]>(this.token || '');
    return tokens;
  }

  get isExpired(): boolean {
    return this.jwt.isTokenExpired(this.token);
  }

  login(userName: string, password: string): Observable<void> {
    const api = 'api/SignIn';
    const data = new FormData();
    data.set('userName', userName);
    data.set('password', password);
    return this.http.post<ApiResult<string>>(api, data).pipe(
      map(result => {
        if (result.success) {
          const token = result.data;
          if (token) {
            localStorage.setItem(this.config.token.name, token);
            this.router.navigateByUrl(this.returnUrl, { replaceUrl: true });
          }
        }
      })
    );
  }

  logout(): Observable<void> {
    const api = 'api/SignOut';
    return this.http.post<ApiResult<void>>(api, {}).pipe(
      map(_ => {
        localStorage.removeItem(this.config.token.name);
        this.router.navigate(['/login'])
          .then(() => window.location.reload());
      }),
      catchError(_ => {
        this.router.navigate(['/login'])
          .then(() => window.location.reload());
        return of();
      })
    );
  }

  refresh(): Observable<void> {
    const api = 'api/Refresh';
    return this.http.post<ApiResult<void>>(api, {}).pipe(
      map(result => {
        if (result.success) {
          const token = result.data;
          if (token)
            localStorage.setItem(this.config.token.name, token);
        }
      }),
      catchError(_ => {
        return of();
      })
    );
  }

  validate(url: string): Observable<boolean> {
    if (this.lastValidateUrl != url) {
      const api = 'api/menu/ValidateMenuItem';
      const data = new FormData();
      data.set('url', url);
      return this.http.post<ApiResult<boolean>>(api, data).pipe(
        map(result => {
          if (result.success) {
            this.lastValidateUrl = url;
            return true;
          }
          return false;
        }),
        catchError(err => {
          if (err && err.status == 401) {
            this.router.navigate(['/error/401'], { queryParams: { dir: url }, replaceUrl: true });
            return of(false);
          } else {
            this.router.navigate(['/error/503'], { queryParams: { dir: url }, replaceUrl: true });
            return of(false);
          }
        })
      );
    }
    return of(true);
  }

  ngOnDestroy(): void {
    this.subscriptions$.forEach(s => s?.unsubscribe());
  }

}
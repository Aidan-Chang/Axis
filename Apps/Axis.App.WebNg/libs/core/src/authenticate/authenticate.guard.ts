import { Injectable, Inject } from '@angular/core';
import { CanLoad, CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot, NavigationCancel } from '@angular/router';
import { lastValueFrom, filter, map } from 'rxjs';
import { AuthenticateService } from '@axis/lib/core';
import { AppConfig } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class AuthenticateGuard implements CanLoad, CanActivate, CanActivateChild {

  constructor(
    @Inject('config') private config: AppConfig,
    private router: Router,
    private auth: AuthenticateService) {
  }

  canLoad(): boolean {
    if (this.auth.isExpired === false) {
      return true;
    } else {
      const s = this.router.events.pipe(
        filter(event => event instanceof NavigationCancel),
        map(event => event as NavigationCancel)
      ).subscribe((event: NavigationCancel) => {
        this.auth.returnUrl = event.url.substring(1);
        this.router.navigate(['/login'], { queryParams: { return_url: event.url.substring(1) }, replaceUrl: true })
          .then(() => window.location.reload());
        s.unsubscribe();
      });
      return false;
    }
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    return this.validate(state.url);
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    return this.validate(state.url);
  }

  async validate(url: string): Promise<boolean> {
    if (this.auth.isExpired === false) {
      if (await lastValueFrom(this.auth.validate(url)))
        return true;
      return false;
    } else {
      localStorage.removeItem(this.config.token.name);
      this.auth.returnUrl = url;
      this.router.navigate(['/login'], { queryParams: { return_url: url.substring(1) }, replaceUrl: true })
        .then(() => window.location.reload());
      return false;
    }
  }

}
import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class RedirectGuard implements CanActivate {

  constructor(private router: Router,) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    this.router.navigate([route.data['url']], { queryParams: { return_url: state.url.substring(1) }, replaceUrl: true });
    return false;
  }

}

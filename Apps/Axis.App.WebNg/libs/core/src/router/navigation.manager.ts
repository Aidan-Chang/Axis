import { Inject, Injectable } from "@angular/core";
import { Location } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { NavigationEnd, Router } from '@angular/router';
import { filter, tap, map, Observable } from 'rxjs';
import { AppConfig } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class NavigationManager {

  event$: Observable<NavigationEnd>;
  private history: string[] = [];

  constructor(
    @Inject('config') config: AppConfig,
    private location: Location,
    private router: Router,
    title: Title) {
    this.event$ = router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(event => event as NavigationEnd),
      tap(event => {
        title.setTitle(config.info?.title || '');
        this.history.push(event.urlAfterRedirects);
        if (this.history.length > 10) {
          this.history.shift();
        }
      }),
    );
  }

  back(): void {
    this.history.pop();
    if (this.history.length > 0) {
      this.location.back();
    } else {
      this.router.navigateByUrl('/');
    }
  }

}
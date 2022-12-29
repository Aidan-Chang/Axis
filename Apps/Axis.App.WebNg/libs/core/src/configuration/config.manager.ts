import { Inject, Injectable, OnDestroy } from "@angular/core";
import { NavigationEnd } from "@angular/router";
import { BehaviorSubject, Observable, ReplaySubject, Subscription } from 'rxjs';
import { PrimeNGConfig } from 'primeng/api';
import { AppConfig, NavigationManager, ThemeManager, ThemeBehaviorSubject, TranslateManager } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class ConfigManager implements OnDestroy {

  current: AppConfig;
  private primeng: PrimeNGConfig;
  private history$: Observable<NavigationEnd>;
  private readonly subscriptions$: [Subscription?] = [];

  constructor(
    @Inject('config') config: AppConfig,
    primeng: PrimeNGConfig,
    navigation: NavigationManager,
    theme: ThemeManager,
    translate: TranslateManager,
  ) {
    this.current = Object.assign({}, config);
    this.primeng = primeng;
    this.current.ripple.value$ = new BehaviorSubject(primeng.ripple);
    this.history$ = navigation.event$;
    this.current.history.value$ = new ReplaySubject();
    this.current.theme.value$ =
      new ThemeBehaviorSubject(
        localStorage.getItem(config.theme.name) || 'light',
        theme.set);
    this.current.language.value$ =
      new BehaviorSubject(
        localStorage.getItem(config.language.name) || translate.service.getBrowserLang()?.toLowerCase() || 'en');
    this.current.environment.value$ = new BehaviorSubject('');
    this.current.developer.value$ =
      new BehaviorSubject(
        (localStorage.getItem(config.developer.name)?.toLowerCase() || 'false') == 'true'
      );
    this.current.token.value$ = new BehaviorSubject('');
  }

  configure(): void {
    this.subscriptions$.push(
    );
  }

  private getBrowserTheme(): string | undefined {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches)
      return 'dark';
    return undefined;
  }

  ngOnDestroy(): void {
    this.subscriptions$.forEach(s => s?.unsubscribe());
  }

}
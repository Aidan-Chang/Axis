import { Inject, Injectable, OnDestroy } from "@angular/core";
import { NavigationEnd } from "@angular/router";
import { BehaviorSubject, Observable, ReplaySubject, Subscription } from 'rxjs';
import { PrimeNGConfig } from 'primeng/api';
import { AppConfig, NavigationManager, } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class ConfigManager implements OnDestroy {

  current: AppConfig;
  private primeng: PrimeNGConfig;
  private history$: Observable<NavigationEnd>;
  private readonly subscriptions$: [Subscription?] = [];

  constructor(
    @Inject('config') input: AppConfig,
    primeng: PrimeNGConfig,
    navigation: NavigationManager,
  ) {
    this.current = Object.assign({}, input);
    this.primeng = primeng;
    this.current.ripple.value$ = new BehaviorSubject(primeng.ripple);
    this.history$ = navigation.event$;
    this.current.history.value$ = new ReplaySubject();
    this.current.theme.value$ = new BehaviorSubject('');
    this.current.language.value$ = new BehaviorSubject('');
    this.current.environment.value$ = new BehaviorSubject('');
    this.current.developer.value$ = new BehaviorSubject(false);
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
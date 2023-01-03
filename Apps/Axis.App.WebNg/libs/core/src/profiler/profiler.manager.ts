import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { ConfigManager } from '@axis/lib/core';

/* eslint-disable @typescript-eslint/no-explicit-any */
declare let MiniProfiler: any;

@Injectable({ providedIn: 'root' })
export class ProfilerManager implements OnDestroy {

  url = '';
  el?: HTMLScriptElement;
  private readonly subscriptions$: [Subscription?] = [];

  constructor(
    private config: ConfigManager) {
    this.url = `${config.current.url.base}${config.current.url.service}`;
    config.current.developer.value$ = new BehaviorSubject(localStorage.getItem(config.current.developer.name) == 'true' ? true : false);
    this.initialize();
    this.subscriptions$.push(
      this.config.current.theme.value$?.subscribe(theme => {
        if (typeof MiniProfiler !== 'undefined') {
          const classlist = MiniProfiler.container.classList;
          switch (theme) {
            case 'light':
              classlist.remove('mp-scheme-dark');
              classlist.add('mp-scheme-light');
              this.el?.setAttribute('data-scheme', theme);
              MiniProfiler.options.colorScheme = theme;
              break;
            case 'dark':
              classlist.remove('mp-scheme-light');
              classlist.add('mp-scheme-dark');
              this.el?.setAttribute('data-scheme', theme);
              MiniProfiler.options.colorScheme = theme;
              break;
          }
        }
      }),
      this.config.current.developer.value$?.subscribe(value => {
        localStorage.setItem(config.current.developer.name, value ? 'true' : 'false');
        localStorage.setItem('MiniProfiler-Display', value ? 'block' : 'none');
        const element = document.getElementsByClassName('mp-results');
        element[0]?.setAttribute('style', `display: ${value ? 'block' : 'none'};`);
      }),
    );
  }

  private initialize(): void {
    let el: HTMLScriptElement = document.getElementById('mini-profiler') as HTMLScriptElement;
    if (!el) {
      el = document.createElement('script');
      el.async = true;
      el.id = 'mini-profiler';
      el.src = `${this.url}/profiler/includes.min.js?v=4.2.0`;
      el.setAttribute('data-version', '4.2.0');
      el.setAttribute('data-path', `${this.url}/profiler/`);
      el.setAttribute('data-current-id', '');
      el.setAttribute('data-ids', '');
      el.setAttribute('data-position', 'Right');
      el.setAttribute('data-scheme', this.config.current.theme.value$?.value || 'light');
      el.setAttribute('data-authorized', 'true');
      el.setAttribute('data-max-traces', '15');
      el.setAttribute('data-toggle-shortcut', 'Alt+P');
      el.setAttribute('data-trivial-milliseconds', '2.0');
      el.setAttribute('data-start-hidden', status ? 'false' : 'true');
      el.setAttribute('data-ignored-duplicate-execute-types', 'Open,OpenAsync,Close,CloseAsync');
      document.head.appendChild(el);
      // set dev mode
      document.addEventListener('keyup', (e: KeyboardEvent) => {
        if (e.altKey && e.key.toLocaleLowerCase() == 'p')
          this.config.current.developer.value$?.next(this.config.current.developer.value$?.value || false);
      });
    }
    this.el = el;
  }

  ngOnDestroy(): void {
    this.subscriptions$.forEach(s => s?.unsubscribe);
  }

}

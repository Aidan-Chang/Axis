import { Inject, Injectable } from '@angular/core';
import { Observable, startWith } from 'rxjs';
import { AppConfig } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class ThemeManager {

  private link1: HTMLLinkElement;
  private link2: HTMLLinkElement;

  constructor(
    @Inject('config') private config: AppConfig) {
    // primeng embeded theme
    const link1 = document.createElement('link');
    link1.rel = 'stylesheet';
    link1.type = 'text/css';
    link1.href = `assets/theme/${this.config.theme.value$?.value || 'light'}/theme.css`;
    document.head.appendChild(link1);
    this.link1 = link1;
    // app stylesheet theme
    const link2 = document.createElement('link');
    link2.rel = 'stylesheet';
    link2.type = 'text/css';
    link2.href = `assets/theme/${this.config.theme.value$?.value || 'light'}.css`;
    document.head.appendChild(link2);
    this.link2 = link2;
  }

  set(value: string): Observable<boolean> {
    return new Observable<boolean>((observer) => {
      let mutation: MutationObserver;
      if (this.link1.href.endsWith(`theme/${value}/theme.css`) == false || this.link2.href.endsWith(`theme/${value}.css`) == false) {
        mutation = new MutationObserver((_) => {
          const checkStatus = () => {
            let link1: CSSStyleSheet | undefined;
            let link2: CSSStyleSheet | undefined;
            for (let i = 0; i < document.styleSheets.length; i++) {
              if (document.styleSheets[i].href == this.link1.href)
                link1 = document.styleSheets[i];
              if (document.styleSheets[i].href == this.link2.href)
                link2 = document.styleSheets[i];
            }
            if (link1 && link2) {
              mutation.disconnect();
              observer.next(true);
              observer.complete();
            }
            else {
              if (observer.closed == false) {
                setTimeout(() => checkStatus(), 100);
              }
            }
          };
          checkStatus();
        });
        mutation.observe(this.link2, { attributes: true, attributeFilter: ['href'] });
        this.link1.href = `assets/theme/${value}/theme.css`;
        this.link2.href = `assets/theme/${value}.css`;
        localStorage.setItem(this.config.theme.value$?.value ?? 'light', value);
      } else {
        observer.next(true);
        observer.complete();
      }
      return {
        unsubscribe() {
          mutation?.disconnect();
        }
      };
    }).pipe(
      startWith(false),
    );
  }

}

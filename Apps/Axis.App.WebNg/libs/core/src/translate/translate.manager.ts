import { Inject, Injectable, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { AppConfig } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class TranslateManager implements OnDestroy {

  langs: { [id: string]: string } = {};
  private subscriptions$: [Subscription?] = [];

  constructor(
    @Inject('config') config: AppConfig,
    public service: TranslateService) {
    this.langs = config?.language.list || {};
    // add languages
    service.addLangs(Object.keys(config?.language.list || {}) || {});
    // set language;
    let value = localStorage.getItem(config.language.name) || service.getBrowserLang()?.toLowerCase() || '';
    if (!this.langs[value])
      value = Object.keys(this.langs)[0];
    service.setDefaultLang(value);
    // apply translate to current language
    this.subscriptions$.push(
      service.use(value).subscribe(_ => {
        localStorage.setItem(config.language.value$?.value || '', value);
      }),
      service.onLangChange.subscribe(result => {
        localStorage.setItem(config.language.value$?.value || '', result.lang);
      }),
    );
  }

  ngOnDestroy(): void {
    this.subscriptions$.forEach(s => s?.unsubscribe());
  }

}

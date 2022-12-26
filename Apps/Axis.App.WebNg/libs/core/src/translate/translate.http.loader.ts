import { HttpClient } from '@angular/common/http';
import { TranslateLoader } from '@ngx-translate/core';
import { Observable, map } from 'rxjs';
import { ApiResult } from '@axis/lib/core';

export class TranslateHttpLoader implements TranslateLoader {

  constructor(
    private http: HttpClient,
    private prefix: string = '/assets/i18n/',
    private suffix: string = '.json') { }

  getTranslation(lang: string): Observable<object | undefined> {
    return this.http.get<ApiResult<object>>(`${this.prefix}${lang}${this.suffix}`).pipe(
      map(result => result.data)
    );
  }

}

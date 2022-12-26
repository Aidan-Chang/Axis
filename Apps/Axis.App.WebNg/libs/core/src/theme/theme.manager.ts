import { Inject, Injectable } from '@angular/core';
import { AppConfig } from '@axis/lib/core';

@Injectable({ providedIn: 'root' })
export class ThemeManager {

  constructor(
    @Inject('config') private config: AppConfig) {
  }

}

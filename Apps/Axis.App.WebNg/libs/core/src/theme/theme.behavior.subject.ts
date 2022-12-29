import { Observable, BehaviorSubject } from 'rxjs';

export class ThemeBehaviorSubject extends BehaviorSubject<string> {

  private timeout?: NodeJS.Timeout;
  private observer: (value: string) => Observable<boolean>;

  constructor(defaultValue: string, observer: (value: string) => Observable<boolean>) {
    super(defaultValue);
    this.observer = observer;
  }

  override next(value: string): void {
    this.timeout = setTimeout(() => {
      if (sub.closed == false) {
        super.next('');
        // this.error(`Change the theme value ${value} failed, the progress is time out`);
      }
    }, 10000);
    const sub = this.observer(value).subscribe(result => {
      if (result) {
        super.next(value);
        sub.unsubscribe();
        clearTimeout(this.timeout);
        this.timeout = undefined;
      }
    });
  }

}
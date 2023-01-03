import { Injectable, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LayoutService implements OnDestroy {

  private readonly subscriptions$: [Subscription?] = [];

  ngOnDestroy(): void {
    this.subscriptions$.forEach(s => s?.unsubscribe());
  }

}
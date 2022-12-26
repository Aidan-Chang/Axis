import { Component, AfterViewInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { PrimeNGConfig } from 'primeng/api';

@Component({
  selector: 'app-root',
  template: `
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent implements AfterViewInit, OnDestroy {

  private readonly subscriptions$: [Subscription?] = [];

  constructor(
    private primeng: PrimeNGConfig,
  ) {
  }

  ngAfterViewInit(): void {
    this.subscriptions$.push(
    );
  }

  ngOnDestroy(): void {
    this.subscriptions$.forEach(s => s?.unsubscribe());
  }

}

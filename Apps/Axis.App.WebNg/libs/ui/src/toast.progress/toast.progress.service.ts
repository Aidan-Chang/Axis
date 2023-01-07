import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { HubConnectionBuilder, HubConnection, HubConnectionState, NullLogger, HttpTransportType } from '@microsoft/signalr';
import { ToastProgressEvent } from './toast.progress.model';

@Injectable({ providedIn: 'root' })
export class ToastProgressService implements OnDestroy {

  private data$ = new Subject<ToastProgressEvent>();
  private disposed = false;
  readonly observer = this.data$.asObservable();
  readonly connection: HubConnection;
  progrssing: string[] = [];

  constructor() {
    const url = 'service/progress';
    this.connection = new HubConnectionBuilder().withUrl(url, {
      // transport: HttpTransportType.WebSockets,
      // skipNegotiation: true,
      logger: NullLogger.instance
    }).build();
    this.connect();
  }

  connect(): void {
  }

  ngOnDestroy() {
  }

}
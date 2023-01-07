import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, finalize } from 'rxjs';
import { HubConnectionState } from '@microsoft/signalr';
import { MessageService } from 'primeng/api';
import { ToastMessage } from './toast.progress.model';
import { ToastProgressService } from './toast.progress.service';

export const toast = <T>(message: ToastMessage) => (source: Observable<T>) => {
  return new Observable<T>(observer => {
    return () => {
    };
  });
}
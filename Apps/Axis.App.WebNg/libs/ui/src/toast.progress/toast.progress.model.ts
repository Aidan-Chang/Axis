import { Subscription } from 'rxjs';
import { Message } from 'primeng/api';

export interface ToastMessage extends Message {
  statusCode?: number,
  progress?: ToastProgress;
  subscriber$?: Subscription;
}

export interface ToastProgress {
  state?: 'processing' | 'completed' | 'canceled' | 'error',
  increment: 'auto' | 'event' | 'none',
  value?: number,
  max?: number,
  expect?: number,
}

export type ToastProgressEvent = {
  id: string;
  value: number;
  max: number;
  except: number;
  summary?: string;
  details?: string;
  data?: object;
}

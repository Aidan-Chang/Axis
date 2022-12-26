import { BehaviorSubject, ReplaySubject } from 'rxjs';

export interface AppConfig {
  readonly info: AppInfo,
  readonly url: {
    base: string,
    service: string,
    api: string,
  },
  readonly environment: {
    name: string,
    value$?: BehaviorSubject<string>,
  },
  readonly theme: {
    name: string,
    list: string[],
    value$?: BehaviorSubject<string>,
  },
  readonly language: {
    name: string,
    list: { [id: string]: string },
    value$?: BehaviorSubject<string>,
  },
  readonly developer: {
    name: string,
    value$?: BehaviorSubject<boolean>,
  },
  readonly ripple: {
    name: string,
    value$?: BehaviorSubject<boolean>,
  },
  readonly token: {
    name: string,
    value$?: BehaviorSubject<string>,
  },
  readonly history: {
    name: string,
    value$?: ReplaySubject<string>,
  }
}

export interface AppInfo {
  title?: string,
  description?: string,
  icon?: string,
  copyright?: string,
  name?: string,
  version?: string,
}
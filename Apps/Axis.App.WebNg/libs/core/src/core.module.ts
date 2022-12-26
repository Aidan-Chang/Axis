import { NgModule, ModuleWithProviders, Optional, SkipSelf, APP_INITIALIZER } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { AppConfig, ConfigManager, ProfilerInterceptor, ProfilerManager } from '@axis/lib/core';

@NgModule({
  declarations: [
  ],
  imports: [
    HttpClientModule,
  ],
  exports: [
  ]
})
export class AxLibCoreModule {
  constructor(@Optional() @SkipSelf() parent: AxLibCoreModule) {
    if (parent) {
      throw new Error(
        "AxLibCoreModule is already loaded. It should only be imported in your application's main module."
      );
    }
  }
  static forRoot(config?: AppConfig): ModuleWithProviders<AxLibCoreModule> {
    return {
      ngModule: AxLibCoreModule,
      providers: [
        MessageService,
        { provide: 'config', useValue: config },
        { provide: APP_INITIALIZER, useFactory: (config: ConfigManager) => config.configure(), deps: [ConfigManager] },
        { provide: HTTP_INTERCEPTORS, useClass: ProfilerInterceptor, deps: [ProfilerManager], multi: true },
      ]
    }
  }
}

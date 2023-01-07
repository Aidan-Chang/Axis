import { Component, AfterViewInit, OnDestroy, ViewEncapsulation, ChangeDetectionStrategy } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'am-ui-toast-item',
  templateUrl: './toast.progress.item.component.html',
  styleUrls: ['./toast.progress.item.component.scss'],
  animations: [
    trigger('messageState', [
      state('visible', style({
        transform: 'translateY(0)',
        opacity: 1
      })),
      transition('void => *', [
        style({ transform: '{{showTransformParams}}', opacity: 0 }),
        animate('{{showTransitionParams}}')
      ]),
      transition('* => void', [
        animate(('{{hideTransitionParams}}'), style({
          height: 0,
          opacity: 0,
          transform: '{{hideTransformParams}}'
        }))
      ])
    ])
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class ToastProgressItemComponent implements AfterViewInit, OnDestroy {

  ngAfterViewInit() {
  }

  ngOnDestroy() {
  }

}
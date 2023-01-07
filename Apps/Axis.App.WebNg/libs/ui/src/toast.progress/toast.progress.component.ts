import { Component, OnInit, AfterViewInit, OnDestroy, ViewEncapsulation, ChangeDetectionStrategy, AfterContentInit } from '@angular/core';
import { trigger, transition, query, animateChild } from '@angular/animations';

@Component({
  selector: 'am-ui-toast',
  templateUrl: './toast.progress.component.html',
  styleUrls: ['./toast.progress.component.scss'],
  animations: [
    trigger('toastAnimation', [
      transition(':enter, :leave', [
        query('@*', animateChild())
      ])
    ])
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class ToastProgressComponent implements OnInit, AfterViewInit, AfterContentInit, OnDestroy {

  ngOnInit() {
  }

  ngAfterViewInit() {
  }

  ngAfterContentInit() {
  }

  ngOnDestroy() {
  }

}
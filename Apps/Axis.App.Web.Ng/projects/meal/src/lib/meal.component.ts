import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CalendarModule } from 'primeng/calendar';

@Component({
  selector: 'axpmeal-meal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ToolbarModule,
    ButtonModule,
    CalendarModule,
    TableModule,
  ],
  template: `
    <div>
      <p-toolbar>
        <div class="p-toolbar-group-start">
          <p-button label="Search" icon="pi pi-search"></p-button>
        </div>
        <div class="p-toolbar-group-middle">
          <p-calendar [(ngModel)]="month" view="month" dateFormat="yy-mm" [readonlyInput]="true" [inputStyle]="{'text-align': 'center'}" (onSelect)="monthChaange($event)"></p-calendar>
        </div>
        <div class="p-toolbar-group-end">
          <p-button label="Export to Excel" icon="pi pi-file-excel"></p-button>
        </div>
      </p-toolbar>
    </div>
    <div>
      <p-table [value]="members" [resizableColumns]="true" styleClass="p-datatable-gridlines" [tableStyle]="{'min-width': '50rem'}">
        <ng-template pTemplate="header">
          <tr>
            <th pResizableColumn>部門代碼</th>
            <th pResizableColumn>部門名稱</th>
            <th pResizableColumn>員工工號</th>
            <th pResizableColumn>員工姓名</th>
            <th *ngFor="let day of days">{{day}}</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-member>
          <tr>
            <td>{{member.departNo}}</td>
            <td>{{member.departName}}</td>
            <td>{{member.eeid}}</td>
            <td>{{member.name}}</td>
            <td *ngFor="let day of days">{{member[day]}}</td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [
  ]
})
export class MealComponent {

  month = new Date();
  members = [{
    departNo: 'A001',
    departName: '吃飯部',
    eeid: '0001',
    name: '張世穎'
  }, {
    departNo: 'A002',
    departName: '睡覺部',
    eeid: '0002',
    name: '張可人'
  }, {
    departNo: 'A002',
    departName: '睡覺部',
    eeid: '0003',
    name: '張力文'
  }, {
    departNo: 'A002',
    departName: '睡覺部',
    eeid: '0004',
    name: '何孟娟'
  }, {
    departNo: 'A002',
    departName: '睡覺部',
    eeid: '0005',
    name: '張世勳'
  }];
  days = Array.from({ length: new Date(this.month.getFullYear(), this.month.getMonth() + 1, 0).getDate() }, (_, i) => i + 1);

  monthChaange(event: Date) {
    this.days = Array.from({ length: new Date(event.getFullYear(), event.getMonth() + 1, 0).getDate() }, (_, i) => i + 1);
  }

}

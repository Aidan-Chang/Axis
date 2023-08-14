import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'axpmeal-meal',
  standalone: true,
  imports: [
    CommonModule,
    ToolbarModule,
    ButtonModule,
    TableModule,
  ],
  template: `
    <div>
      <p-toolbar>
        <div class="p-toolbar-group-start">
          <p-button label="Search" icon="pi pi-search"></p-button>
        </div>
        <div class="p-toolbar-group-end">
          <p-button label="Export to Excel" icon="pi pi-file-excel"></p-button>
        </div>
      </p-toolbar>
    </div>
    <div>
      <p-table>
      </p-table>
    </div>
  `,
  styles: [
  ]
})
export class MealComponent {

}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'axapp-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <h1>
      我要訂餐
    </h1>
    <div>
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [],
})
export class AppComponent {
  title = '我要訂餐';
}

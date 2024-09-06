import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-error-card',
  templateUrl: './error-card.component.html',
  styleUrls: ['./error-card.component.css']
})
export class ErrorCardComponent {
  @Input() title: string = 'Error';
  @Input() message: string = 'Ha ocurrido un error inesperado.';
}






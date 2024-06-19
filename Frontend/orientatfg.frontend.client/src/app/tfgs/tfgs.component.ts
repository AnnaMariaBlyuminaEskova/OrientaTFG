import { Component, HostListener } from '@angular/core';

@Component({
  selector: 'app-tfgs',
  templateUrl: './tfgs.component.html',
  styleUrls: ['./tfgs.component.css']
})
export class TFGsComponent {
  isDropdownOpen = false;

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  onClick(event: Event) {
    const target = event.target as HTMLElement;
    const profileContainer = document.querySelector('.profile-container');

    if (!profileContainer?.contains(target)) {
      this.isDropdownOpen = false;
    }
  }
}


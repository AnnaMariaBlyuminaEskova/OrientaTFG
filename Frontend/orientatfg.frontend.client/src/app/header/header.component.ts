import { Component, HostListener, OnInit } from '@angular/core';
import { Router } from '@angular/router';

enum RoleEnum {
  Estudiante = 1,
  Tutor = 2,
  Administrador = 3
}

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})

export class HeaderComponent implements OnInit {
  profilePicture: string = '';
  token: boolean = false;
  isDropdownOpen: boolean = false;
  userRole: string = '';
  tfgId: number | null = null; 

  constructor(private router: Router) { }

  ngOnInit() {
    this.profilePicture = localStorage.getItem('profilePicture') || '';
    if (localStorage.getItem('token')) {
      this.token = true;
    }
    const role = localStorage.getItem('role');
    if (role) {
      const parsedRole = parseInt(role, 10);
      if (parsedRole === RoleEnum.Estudiante) {
        this.userRole = 'Estudiante';
      } else if (parsedRole === RoleEnum.Tutor) {
        this.userRole = 'Tutor';
      } else {
        this.userRole = 'Administrador';
      }
    }
    const tfgIdStr = localStorage.getItem('tfgId');
    this.tfgId = tfgIdStr ? parseInt(tfgIdStr, 10) : null;
  }

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

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('profilePicture');
    localStorage.removeItem('id');
    localStorage.removeItem('tfgId');
    localStorage.removeItem('role');
    this.router.navigate(['/login']);
  }

  navigateToChat() {
    if (this.userRole === 'Estudiante' && this.tfgId !== null) {
      this.router.navigate(['/chat', this.tfgId]);
    }
  }

  navigateToProfile() {
    this.router.navigate(['/perfil']);
  }
}





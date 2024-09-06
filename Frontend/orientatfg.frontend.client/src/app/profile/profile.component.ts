import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Environment } from '../../environment'; 

interface ProfileData {
  id?: number;
  name: string;
  surname: string;
  email: string;
  alertEmail?: boolean;
  calificationEmail?: boolean;
  totalTaskHours?: number;
  anticipationDaysForFewerThanTotalTaskHoursTasks?: number;
  anticipationDaysForMoreThanTotalTaskHoursTasks?: number;
}

enum RoleEnum {
  Estudiante = 1,
  Tutor = 2,
  Administrador = 3
}

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css', '../app.component.css']
})

export class ProfileComponent implements OnInit{
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;
  profileData?: ProfileData;
  userRole: string = '';

  constructor(private http: HttpClient, private router: Router) { }

  ngOnInit() {
    const token = localStorage.getItem('token');
    const id = localStorage.getItem('id');
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

    if (token && id && role) {
      this.getProfileData(id, token, role);
    }
  }

  getProfileData(id: string, token: string, role: string) {

    const apiUrl = role === '2'
      ? `${Environment.userApiUrl}/tutor-profile/${id}`
      : `${Environment.userApiUrl}/student-profile/${id}`;

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.http.get<ProfileData>(apiUrl, { headers }).subscribe(
      (data) => {
        this.profileData = data;
        this.isLoading = false;
      },
      (error) => {
        console.error('Get profile data error:', error);
      }
    );
  }

  onSubmit(form: NgForm) {
    const token = localStorage.getItem('token');
    const id = localStorage.getItem('id');
    const role = localStorage.getItem('role');

    this.isLoading = true;

    const apiUrl = role === '2'
      ? `${Environment.userApiUrl}/tutor-profile`
      : `${Environment.userApiUrl}/student-profile`;

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    if (!this.profileData) return;
    if (id) {
      this.profileData.id = parseInt(id);
      this.profileData.email = form.value.email;

      if (role == '1')
      {
        this.profileData.alertEmail = form.value.alertEmail;
        this.profileData.calificationEmail = form.value.calificationEmail;
        this.profileData.totalTaskHours = form.value.totalTaskHours;
        this.profileData.anticipationDaysForFewerThanTotalTaskHoursTasks = form.value.anticipationDaysForFewerThanTotalTaskHoursTasks;
        this.profileData.anticipationDaysForMoreThanTotalTaskHoursTasks = form.value.anticipationDaysForMoreThanTotalTaskHoursTasks;
      }
      this.http.put(apiUrl, this.profileData, { headers }).subscribe(
        () => {
          this.isLoading = false;
          this.successMessage = 'Los cambios se han guardado correctamente.';
        },
        (error) => {
          this.errorMessage = 'No se pudieron guardar los cambios. Por favor, inténtelo de nuevo más tarde.';
          this.isLoading = false;
        }
      );
    }
  }
}


import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Environment } from '../../environment'; 
import { Location } from '@angular/common';
interface RegisterData {
  name: string;
  surname: string;
  email: string;
  password: string;
  profilePicture: string;
  profilePictureName: string;
  departmentId: number;
}

interface Department {
  id: number;
  name: string;
}
interface RegisterResponse {
  error?: string;
}

@Component({
  selector: 'app-register-tutor',
  templateUrl: './register-tutor.component.html',
  styleUrls: ['./register-tutor.component.css', '../app.component.css']
})

export class RegisterTutorComponent implements OnInit {
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;
  selectedFile: File | null = null;
  photoBase64: string = '';
  departments: Department[] = [];
  selectedDepartmentId: number | null = null;

  constructor(private http: HttpClient, private router: Router, private location: Location) { }

  ngOnInit() {
    const token = localStorage.getItem('token');
    if (token) {
      this.getDepartments(token);
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.convertFileToBase64(file);
    }
  }

  convertFileToBase64(file: File): void {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      this.photoBase64 = reader.result as string;
    };
    reader.onerror = (error) => {
      console.error('Error converting file to base64:', error);
    };
  }

  onSubmit(form: NgForm) {
    this.errorMessage = '';

    if (form.invalid) {
      return;
    }
    if (!this.selectedDepartmentId) {
      this.errorMessage = 'Por favor, seleccione un departamento.';
      return;
    }
    if (!this.selectedFile) {
      this.errorMessage = 'Por favor, seleccione una foto de perfil.';
      return;
    }

    const token = localStorage.getItem('token');
    if (token) {

      this.isLoading = true;

      const registerData: RegisterData = {
        name: form.value.name,
        surname: form.value.surname,
        email: form.value.email,
        password: form.value.password,
        profilePicture: this.photoBase64,
        profilePictureName: this.selectedFile.name,
        departmentId: this.selectedDepartmentId! 
      };

      const headers = new HttpHeaders({
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      });

      this.http.post<RegisterResponse>(`${Environment.userApiUrl}/register-tutor`, registerData, { headers }).subscribe(
        (response) => {
          this.isLoading = false;
          if (response.error) {
            this.errorMessage = response.error;
          }
          else {
            this.successMessage = 'El tutor ha sido registrado correctamente.';
          }
        },
        (error) => {
          this.isLoading = false;
          this.errorMessage = 'Ha ocurrido un error, inténtelo de nuevo más tarde';
        }
      );
    }
  }

  getDepartments(token: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.http.get<Department[]>(`${Environment.userApiUrl}/departments`, { headers }).subscribe(
      (data) => {
        this.departments = data;
        this.isLoading = false;
      },
      (error) => {
        console.error('Get departments error:', error);
      }
    );
  }

  goBack() {
    this.location.back();
  }
}


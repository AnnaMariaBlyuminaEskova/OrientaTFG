import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Environment } from '../../environment'; 

interface RegisterData {
  name: string;
  surname: string;
  email: string;
  password: string;
  profilePicture: string;
  profilePictureName: string;
}

interface LogInResponse {
  error?: string;
  token?: string;
  id?: number;
  profilePicture: string;
  role?: number;
}

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css', '../app.component.css']
})

export class RegisterComponent {
  errorMessage: string = '';
  isLoading: boolean = false;
  selectedFile: File | null = null;
  photoBase64: string = '';

  constructor(private http: HttpClient, private router: Router) { }

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
    if (!this.selectedFile) {
      this.errorMessage = 'Por favor, seleccione una foto de perfil.';
      return;
    }

    this.isLoading = true;

    const registerData: RegisterData = {
      name: form.value.name,
      surname: form.value.surname,
      email: form.value.email,
      password: form.value.password,
      profilePicture: this.photoBase64,
      profilePictureName: this.selectedFile.name
    };

    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    this.http.post<LogInResponse>(`${Environment.userApiUrl}/register-student`, registerData, { headers }).subscribe(
      (response) => {
        this.isLoading = false;
        if (response.error) {
          this.errorMessage = response.error;
        } else if (response.token) {
          localStorage.setItem('token', response.token);
          if (response.id !== undefined) {
            localStorage.setItem('id', response.id.toString());
          }
          if (response.profilePicture) {
            localStorage.setItem('profilePicture', response.profilePicture);
          }
          if (response.role !== undefined) {
            localStorage.setItem('role', response.role.toString());
            this.router.navigate(['/panel']);
          }
        }
      },
      (error) => {
        this.isLoading = false;
        console.error('Register error:', error);
      }
    );
  }
}


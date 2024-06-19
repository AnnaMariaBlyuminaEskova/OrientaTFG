import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

interface LogInData {
  email: string;
  password: string;
}

interface LogInResponse {
  error?: string;
  token?: string;
}

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  constructor(private http: HttpClient, private router: Router, private route: ActivatedRoute) { console.log('Current URL:', this.route.snapshot.url); }

  onSubmit(form: NgForm) {

    const logInData: LogInData = { email: form.value.email, password: form.value.password };

    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    this.http.post<LogInResponse>('http://localhost:5247/login', logInData, { headers }).subscribe(
      (response) => {
        if (response.error) {
          this.errorMessage = response.error;
        } else if (response.token) {
          localStorage.setItem('token', response.token);
          this.router.navigate(['/tfgs']);
        }
      },
      (error) => {
        console.error('Error al iniciar sesión:', error);
      }
    );
  }
}


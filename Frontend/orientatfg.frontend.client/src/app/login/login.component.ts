import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Environment } from '../../environment'; 
import { ChatService } from '../chat/chat.service';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
interface LogInData {
  email: string;
  password: string;
}

interface LogInResponse {
  error?: string;
  token?: string;
  id?: number;
  profilePicture: string;
  role?: number;
}

enum RoleEnum {
  Estudiante = 1,
  Tutor = 2,
  Administrador = 3
}

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css', '../app.component.css']
})
export class LoginComponent {
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(private http: HttpClient, private router: Router, private chatService: ChatService) { }

  onSubmit(form: NgForm) {
    this.isLoading = true;
    this.errorMessage = '';

    const logInData: LogInData = { email: form.value.email, password: form.value.password };

    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    this.http.post<LogInResponse>(`${Environment.userApiUrl}/login`, logInData, { headers }).subscribe(
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

            if (response.role == RoleEnum.Estudiante) {


              this.getStudentTFGId(response.id!.toString(), response.token).subscribe(
                (response) => {
                  if (response) {
                    localStorage.setItem('tfgId', response.toString());
                    this.router.navigate(['/panel', response.toString()]);
                  }
                },
                (error) => {
                  if (error.status === 404) {
                    this.router.navigate(['/panel']);
                  }
                  else {
                    this.errorMessage = 'Ha ocurrido un error inesperado, por favor, inténtalo de nuevo más tarde.';
                  }
                }
              );

              this.router.navigate(['/panel']);
            }
            else if (response.role == RoleEnum.Tutor) {
              this.router.navigate(['/TFGs']);
            }
            else if (response.role == RoleEnum.Administrador) {
              this.router.navigate(['/tutores']);
            }
          }
        }
      },
      (error) => {
        this.isLoading = false;
        console.error('Login error:', error);
      }
    );
  }

  getStudentTFGId(id: string, token: string): Observable<any> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.get<object>(`${Environment.tfgApiUrl}/student/${id}`, { headers }).pipe(
      tap((data) => {
      }),
      catchError((error) => {
        console.error('Get student TFG id error:', error);
        return throwError(error);
      })
    );
  }

  register() {
    this.router.navigate(['/registro']);
  }
}


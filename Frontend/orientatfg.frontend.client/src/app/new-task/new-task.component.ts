import { Component, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Environment } from '../../environment';
import { Location } from '@angular/common';

interface MainTask {
  id?: number;
  tfgId: number;
  name: string;
  deadline: string;
  maximumPoints: number;
  description: string;
}

@Component({
  selector: 'app-new-task',
  templateUrl: './new-task.component.html',
  styleUrls: ['./new-task.component.css', '../app.component.css']
})
export class NewTaskComponent implements OnInit{
  isEditMode: boolean = false;
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  task: MainTask = {
    tfgId: 0,
    name: '',
    deadline: '',
    maximumPoints: 0,
    description: ''
  };
  hasError: boolean = false;
  errorMessageBack = '';

  constructor(private router: Router, private http: HttpClient, private location: Location) { }

  ngOnInit(): void {
    const state = history.state;
    if (state && state.task) {
      const task = state.task as MainTask;
      this.isEditMode = true;
      this.task = task;
      if (this.task.deadline) {
        this.task.deadline = this.task.deadline.substring(0, 16);
      }
    }
  }

  onSubmit(form: NgForm) {
    if (!form.valid) {
      this.errorMessage = 'Por favor, complete todos los campos correctamente';
      return;
    }

    const date = form.value.date;

    const currentDate = new Date();
    const inputDate = new Date(date);

    if (inputDate < currentDate) {
      this.errorMessage = 'La fecha de entrega no puede ser anterior a la actual';
      return;
    }

    this.isLoading = true;
    const token = localStorage.getItem('token');
    const tfgId = localStorage.getItem('tfgId');
    if (tfgId && token) {
      const parsedTfgId = parseInt(tfgId);

      const mainTask: MainTask = {
        id: this.isEditMode ? this.task.id : undefined,
        tfgId: parsedTfgId,
        name: form.value.title,
        deadline: date,
        maximumPoints: form.value.points,
        description: form.value.description
      };
      if (this.isEditMode) {
        this.updateMainTask(mainTask, form, token);
      } else {
        this.createMainTask(mainTask, form, token);
      }
    }

  }
  createMainTask(mainTask: MainTask, form: NgForm, token: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.http.post<MainTask>(`${Environment.tfgApiUrl}/main-task`, mainTask, { headers }).pipe(
      catchError(error => {
        this.isLoading = false;
        this.errorMessage = 'Error al crear la tarea. Por favor, inténtalo de nuevo.';
        return throwError(error);
      })
    ).subscribe(
      (response) => {
        this.isLoading = false;
        this.successMessage = 'La tarea se ha creado correctamente.';
        form.reset();
      }
    );
  }

  updateMainTask(mainTask: MainTask, form: NgForm, token: string): void {

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.http.put<MainTask>(`${Environment.tfgApiUrl}/main-task`, mainTask, { headers }).pipe(
      catchError(error => {
        this.isLoading = false;
        this.errorMessage = 'Error al actualizar la tarea. Por favor, inténtalo de nuevo.';
        return throwError(error);
      })
    ).subscribe(
      (response) => {
        this.isLoading = false;
        this.successMessage = 'La tarea se ha actualizado correctamente.';
      }
    );  
  }

  goBack() {
    this.location.back();
  }
}





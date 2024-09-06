import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Environment } from '../../environment';
import { Location } from '@angular/common';

interface TFG {
  name: string;
  tutorId: number;
  studentId: number;
}

interface previousTFG {
  id: number;
  studentName: string;
  studentSurname: string;
  studentProfilePicture: string;
  name: string;
  pendingTasks: number;
  mainTasksNotEvaluated: number;
  tasksInProgress: number;
  subTasksToDo: number;
}

interface Student {
  id: number;
  name: string;
  surname: string;
  profilePicture: string;
  tfg: boolean;
}

@Component({
  selector: 'app-new-tfg',
  templateUrl: './new-tfg.component.html',
  styleUrls: ['./new-tfg.component.css', '../app.component.css']
})
export class NewTFGComponent implements OnInit{
  errorMessage: string = '';
  successMessage: string = '';
  studentFilter: string = '';
  selectedStudent: Student | null = null;
  students: Student[] = [];
  filteredStudents: Student[] = [];
  isLoading = true;
  isEditMode: boolean = false; 
  tfgId: number | null = null;
  title: string = '';

  constructor(private router: Router, private http: HttpClient, private location: Location, private route: ActivatedRoute) { }

  ngOnInit() {
    const state = history.state;

    if (state && state.tfg) {
      const tfg = state.tfg as previousTFG;
      this.isEditMode = true;
      this.tfgId = tfg.id;
      this.selectedStudent = {
        id: 0,
        name: tfg.studentName,
        surname: tfg.studentSurname,
        profilePicture: tfg.studentProfilePicture,
        tfg: true
      };
      this.title = tfg.name;
      this.isLoading = false;
    } else {
        const token = localStorage.getItem('token');
        if (token) {
          this.getStudents(token);
        }
      }
  }

  onSubmit(form: NgForm) {
    if (this.selectedStudent) {
      const id = localStorage.getItem('id');
      const token = localStorage.getItem('token');

      if (id && token) {
        const parsedId = parseInt(id);

        const tfg: TFG = {
          name: form.value.title,
          tutorId: parsedId,
          studentId: this.selectedStudent.id
        };

        if (this.isEditMode && this.tfgId) {
          this.updateTFG(tfg, token);
        } else {
          this.createTFG(tfg, token);
          form.reset();
          this.selectedStudent = null;
        }
      }
    } else {
      this.errorMessage = 'Por favor, seleccione un estudiante';
    }
  }

  filterStudents(filter: string) {
    this.studentFilter = filter.toLowerCase();
    this.filteredStudents = this.students.filter(student => {
      const fullName = `${student.name.toLowerCase()} ${student.surname.toLowerCase()}`;
      return (
        student.name.toLowerCase().includes(this.studentFilter) ||
        student.surname.toLowerCase().includes(this.studentFilter) ||
        fullName.includes(this.studentFilter)
      );
    });
  }

  selectStudent(student: Student) {
    this.selectedStudent = student;
    this.studentFilter = '';
    this.filteredStudents = [];
  }

  deselectStudent() {
    this.selectedStudent = null;
  }

  getStudents(token: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.http.get<Student[]>(`${Environment.userApiUrl}/students`, { headers }).subscribe(
      (data) => {
        this.students = data;
        this.filteredStudents = data;
        this.isLoading = false;
      },
      (error) => {
        console.error('Get students error:', error);
      }
    );
  }
  
  createTFG(tfg: TFG, token: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.http.post<TFG>(`${Environment.tfgApiUrl}`, tfg, { headers }).pipe(
      catchError(error => {
        this.isLoading = false;
        this.errorMessage = 'Error al crear el tfg. Por favor, inténtalo de nuevo.';
        return throwError(error);
      })
    ).subscribe(
      (response) => {
        this.isLoading = false;
        this.errorMessage = '';
        this.successMessage = 'El tfg se ha creado correctamente.';
      }
    );
  }

  updateTFG(tfg: TFG, token: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    const body = JSON.stringify(tfg.name);

    this.http.put(`${Environment.tfgApiUrl}/${this.tfgId}`, body, { headers }).pipe(
      catchError(error => {
        this.isLoading = false;
        this.errorMessage = 'Error al actualizar el TFG. Por favor, inténtalo de nuevo.';
        return throwError(error);
      })
    ).subscribe(
      (response) => {
        this.isLoading = false;
        this.errorMessage = '';
        this.successMessage = 'El TFG se ha actualizado correctamente.';
      }
    );
  }

  goBack() {
    this.location.back();
  }
}






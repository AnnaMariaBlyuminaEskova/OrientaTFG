import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, HostListener, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Environment } from '../../environment';
import { Location } from '@angular/common';

enum RoleEnum {
  Estudiante = 1,
  Tutor = 2
}

interface FileDTO {
  name: string;
  content: string;
}

interface CommentDTO {
  text: string | null;
  files: FileDTO[] | null;
  mainTaskId: number | null;
  subTaskId: number | null;
}

interface FileCommentHistoryDTO {
  name: string;
  url: string;
}

interface CommentReceivedDTO {
  text: string;
  files: FileCommentHistoryDTO[];
  createdBy: string;
  createdOn: Date;
}

interface CommentHistoryDTO {
  comments: CommentReceivedDTO[];
  taskName: string;
}

@Component({
  selector: 'app-task-detail',
  templateUrl: './task-detail.component.html',
  styleUrls: ['./task-detail.component.css', '../app.component.css']
})
export class TaskDetailComponent implements OnInit {
  taskData: any = {};
  userRole: string = '';
  newComment: string = '';
  selectedFiles: File[] = [];
  taskId: number = 0;
  commentHistory: CommentHistoryDTO | null = null;
  isLoading: boolean = true;
  isMainTask: boolean = false;
  hasError: boolean = false;
  errorMessage = '';
  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient, private location: Location) { }

  ngOnInit() {
    const role = localStorage.getItem('role');
    if (role) {
      const parsedRole = parseInt(role);

      if (parsedRole == RoleEnum.Estudiante) {
        this.userRole = 'Estudiante';
      }
      else {
        this.userRole = 'Tutor';
      }
    }
    this.route.params.subscribe(params => {
      this.taskId = params['id'];
    });
    this.route.url.subscribe(urlSegments => {
      this.isMainTask = urlSegments.some(segment => segment.path === 'detalle-tarea');

      this.getCommentHistory();
    });
  }

  getCommentHistory() {
    const token = localStorage.getItem('token');
    if (token) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });

      this.http.get<CommentHistoryDTO>(`http://localhost:5268/comment-history/${this.taskId}/${this.isMainTask}`, { headers }).subscribe(
        (data) => {
          this.commentHistory = data;
          this.isLoading = false;
        },
        (error) => {
          this.isLoading = false;
          this.hasError = true;
          if (error.status === 404) {
            this.errorMessage = "La página que buscas no existe."
          }
          else if (error.status === 401) {
            localStorage.removeItem('token');
            localStorage.removeItem('profilePicture');
            localStorage.removeItem('id');
            localStorage.removeItem('tfgId');
            localStorage.removeItem('role');
            this.router.navigate(['/login']);
          }
          else if (error.status === 403) {
            this.errorMessage = "Ups, no tienes permisos para visualizar esta página."
          }
          else {
            this.errorMessage = 'Ha ocurrido un error inesperado, por favor, inténtalo de nuevo más tarde.';
          }
        }
      );
    }
  }

  onFilesSelected(event: any) {
    if (event.target.files) {
      const newFiles: File[] = Array.from(event.target.files);
      this.selectedFiles = this.selectedFiles.concat(newFiles);
    }
  }

  convertFileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve((reader.result as string));
      reader.onerror = error => reject(error);
    });
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  async submitComment() {
    if (this.newComment.trim() || this.selectedFiles.length > 0) {
      const fileDTOs: FileDTO[] = await Promise.all(
        this.selectedFiles.map(async (file) => ({
          name: file.name,
          content: await this.convertFileToBase64(file)
        })));

      const commentDTO: CommentDTO = {
        text: this.newComment || null,
        files: fileDTOs.length ? fileDTOs : null,
        mainTaskId: this.isMainTask ? this.taskId || null : null,
        subTaskId: !this.isMainTask ? this.taskId || null : null
      };

      const token = localStorage.getItem('token');

      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });

      this.http.post<CommentDTO>(`http://localhost:5268/comment`, commentDTO, { headers }).subscribe(
        (data) => {
          this.getCommentHistory();
        },
        (error) => {
          console.error('Get students error:', error);
        }
      );

      this.newComment = '';
      this.selectedFiles = [];
    }
  }

  formatDate(date: any): string {

    if (!(date instanceof Date)) {
      date = new Date(date);
    }

    const days = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

    const dayName = days[date.getDay()];
    const day = date.getDate();
    const month = months[date.getMonth()];
    const year = date.getFullYear();

    const hours = this.pad(date.getHours());
    const minutes = this.pad(date.getMinutes());

    return `${dayName} ${day} de ${month} de ${year} a las ${hours}:${minutes}`;
  }

  pad(number: number): string {
    return number < 10 ? '0' + number : number.toString();
  }

  goBack() {
    this.location.back();
  }
}






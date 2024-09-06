import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, HostListener, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Environment } from '../../environment';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

interface ProfileData {
  alertEmail?: boolean;
  totalTaskHours?: number;
  anticipationDaysForFewerThanTotalTaskHoursTasks?: number;
  anticipationDaysForMoreThanTotalTaskHoursTasks?: number;
}
interface MainTaskSummary {
  id: number;
  name: string;
  order: string;
  description: string;
  deadline: string;
  status: string;
  maximumPoints: number;
  obtainedPoints: number;
  subTasksToDo: number;
  hoursSubTasksToDo: number;
  doneSubTasks: number;
  hoursDoneSubTasks: number;
}

interface Summary {
  name: string;
  taskCount: number;
  totalObtainedPoints: number;
  totalMaximumPoints: number;
  subTaskCount: number;
  subTaskHours: number;
  doneSubTasks: number;
  hoursDoneSubTasks: number;
  subTasksToDo: number;
  hoursSubTasksToDo: number;
}

interface TFGSummary {
  name: string;
  mainTaskSummaryDTOList: MainTaskSummary[];
}

enum RoleEnum {
  Estudiante = 1,
  Tutor = 2,
  Administrador = 3
}

@Component({
  selector: 'app-panel-tfg',
  templateUrl: './panel-tfg.component.html',
  styleUrls: ['./panel-tfg.component.css', '../app.component.css']
})

export class PanelTFGComponent implements OnInit {
  mainTaskSummaries: MainTaskSummary[] = [];
  filteredMainTaskSummaries: MainTaskSummary[] = [];
  searchName: string = '';
  searchDate: string = '';
  tfgName: string = '';
  isLoading = true;
  summary: Summary = {
    name: '',
    taskCount: 0,
    totalObtainedPoints: 0,
    totalMaximumPoints: 0,
    subTaskCount: 0,
    subTaskHours: 0,
    doneSubTasks: 0,
    hoursDoneSubTasks: 0,
    subTasksToDo: 0,
    hoursSubTasksToDo: 0
  };
  showConfirmModal: boolean = false;
  taskIdToDelete: number | null = null;
  statuses: string[] = ['Desarrollo', 'Pendiente', 'Realizado'];
  selectedStatus: string[] = [];
  dropdownOpen = false;
  profileData?: ProfileData;
  hasError = false;
  errorMessage = '';

  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient) { }

  ngOnInit() {
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('id');
    const userRole = localStorage.getItem('role');
    const tfgId = this.route.snapshot.paramMap.get('id');

    if (token && userId && userRole && tfgId) {

      const parsedRole = parseInt(userRole, 10);

      if (parsedRole == RoleEnum.Estudiante) {
        this.getProfileData(userId, token);
      }
      this.getTFGSummary(parseInt(tfgId), token);
    }
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

  getTFGSummary(id: number, token: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.http.get<TFGSummary>(`${Environment.tfgApiUrl}/${id}`, { headers }).subscribe(
      (data) => {
        this.mainTaskSummaries = data.mainTaskSummaryDTOList;
        this.filteredMainTaskSummaries = data.mainTaskSummaryDTOList;
        this.tfgName = data.name;
        this.calculateSummary();
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

  getProfileData(id: string, token: string) {

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.http.get<ProfileData>(`${Environment.userApiUrl}/student-profile/${id}`, { headers }).subscribe(
      (data) => {
        this.profileData = data;
      },
      (error) => {
        console.error('Get profile data error:', error);
      }
    );
  }

  shouldApplyRedText(mainTaskSummary: any): boolean {
    const deadline = new Date(mainTaskSummary.deadline);
    const today = new Date();

    return mainTaskSummary.status !== 'Realizado' && deadline.getTime() < today.getTime();
  }

  shouldShowAlertIcon(mainTaskSummary: any): boolean {
    const taskHours = mainTaskSummary.hoursSubTasksToDo + mainTaskSummary.hoursDoneSubTasks;
    const deadline = new Date(mainTaskSummary.deadline);
    const today = new Date();
    const daysUntilDeadline = Math.ceil((deadline.getTime() - today.getTime()) / (1000 * 3600 * 24));

    if (deadline.getTime() < today.getTime()) {
      return false;
    }

    return mainTaskSummary.status !== 'Realizado' && ((taskHours < this.profileData?.totalTaskHours! && daysUntilDeadline <= this.profileData?.anticipationDaysForFewerThanTotalTaskHoursTasks!) ||
      (taskHours >= this.profileData?.totalTaskHours! && daysUntilDeadline <= this.profileData?.anticipationDaysForMoreThanTotalTaskHoursTasks!))
  }


  calculateSummary(): void {
    this.summary.name = this.tfgName;
    this.summary.taskCount = this.mainTaskSummaries.length;
    this.summary.totalObtainedPoints = this.mainTaskSummaries.reduce((sum, task) => sum + task.obtainedPoints, 0);
    this.summary.totalMaximumPoints = this.mainTaskSummaries.reduce((sum, task) => sum + task.maximumPoints, 0);

    this.summary.subTaskCount = this.mainTaskSummaries.reduce((sum, task) => sum + task.subTasksToDo + task.doneSubTasks, 0);
    this.summary.subTaskHours = this.mainTaskSummaries.reduce((sum, task) => sum + task.hoursSubTasksToDo + task.hoursDoneSubTasks, 0);
    this.summary.doneSubTasks = this.mainTaskSummaries.reduce((sum, task) => sum + task.doneSubTasks, 0);
    this.summary.hoursDoneSubTasks = this.mainTaskSummaries.reduce((sum, task) => sum + task.hoursDoneSubTasks, 0);
    this.summary.subTasksToDo = this.mainTaskSummaries.reduce((sum, task) => sum + task.subTasksToDo, 0);
    this.summary.hoursSubTasksToDo = this.mainTaskSummaries.reduce((sum, task) => sum + task.hoursSubTasksToDo, 0);

    this.isLoading = false;
  }

  navigateToNewTask() {
    this.router.navigate(['/nueva-tarea']);
  }

  navigateToTask(taskId: number) {
    this.router.navigate([`/tarea/${taskId}`]);
  }

  deleteTask(taskId: number) {
    this.taskIdToDelete = taskId;
    this.showConfirmModal = true;
  }

  confirmDelete() {
    if (this.taskIdToDelete) {
      const token = localStorage.getItem('token');
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });

      this.http.delete(`${Environment.tfgApiUrl}/main-task/${this.taskIdToDelete}`, { headers }).subscribe(
        () => {
          this.showConfirmModal = false;
          this.router.navigate([this.router.url]);
        },
        (error) => {
          console.error('Error al eliminar la tarea:', error);
          this.showConfirmModal = false;
        }
      );
    }
  }

  sortTable(column: keyof MainTaskSummary, direction: 'asc' | 'desc') {
    this.filteredMainTaskSummaries.sort((a, b) => {
      let first = a[column];
      let second = b[column];

      if (column === 'deadline') {
        first = new Date(first).getTime();
        second = new Date(second).getTime();
      }

      if (first < second) {
        return direction === 'asc' ? -1 : 1;
      } else if (first > second) {
        return direction === 'asc' ? 1 : -1;
      } else {
        return 0;
      }
    });
  }

  toggleStatus(statusName: string): void {
    const index = this.selectedStatus.indexOf(statusName);
    if (index === -1) {
      this.selectedStatus.push(statusName);
    } else {
      this.selectedStatus.splice(index, 1);
    }
    this.filterTasks();
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  filterTasks(): void {
    const searchNameLower = this.searchName.toLowerCase();

    this.filteredMainTaskSummaries = this.mainTaskSummaries.filter(task => {
      const matchesName = task.name.toLowerCase().includes(searchNameLower)
      const matchesDate = task.deadline.includes(this.searchDate)
      const matchesStatus = this.selectedStatus.length === 0 ||
        this.selectedStatus.includes(task.status);

      return matchesName && matchesDate && matchesStatus;
    });
  }

  closeModal() {
    this.showConfirmModal = false;
    this.taskIdToDelete = null;
  }

  editTask(task: any) {
    this.router.navigate(['/edición-tarea', task.id], { state: { task } });
  }
}




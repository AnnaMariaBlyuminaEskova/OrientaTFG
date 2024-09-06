import { Router } from '@angular/router';
import { HostListener, Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Environment } from '../../environment';

enum RoleEnum {
  Estudiante = 1,
  Tutor = 2
}

interface TFG {
  id: number;
  studentName: string;
  studentSurname: string;
  studentProfilePicture: string;
  name: string;
  delayedMainTasks: number;
  mainTasksNotEvaluated: number;
  tasksInProgress: number;
  subTasksToDo: number;
}

@Component({
  selector: 'app-tfgs',
  templateUrl: './tfgs.component.html',
  styleUrls: ['./tfgs.component.css', '../app.component.css']
})

export class TFGsComponent implements OnInit {
  isLoading = true;
  tfgs: TFG[] = [];
  filteredTfgs: TFG[] = [];
  searchName: string = '';
  searchTFG: string = '';
  showConfirmModal: boolean = false;
  tfgIdToDelete: number | null = null;
  userRole: string = '';

  constructor(private router: Router, private http: HttpClient) { }

  ngOnInit() {
    const token = localStorage.getItem('token');
    const id = localStorage.getItem('id');

    if (token && id) {
      this.getTFGsData(id, token);
    }
  }

  getTFGsData(id: string, token: string) {
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
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.http.get<TFG[]>(`${Environment.tfgApiUrl}/all/${id}`, { headers }).subscribe(
      (data) => {
        this.tfgs = data;
        this.filteredTfgs = data;
        this.isLoading = false;
        /*setTimeout(() => {
          this.isLoading = false;
        }, 20000);*/
      },
      (error) => {
        console.error('Get TFGs error:', error);
      }
    );
  }

  navigateToNewTFG() {
    this.router.navigate(['/nuevo-TFG']);
  }

  navigateToPanel(tfgId: number) {
    localStorage.setItem('tfgId', tfgId.toString());
    this.router.navigate([`/panel`, tfgId]);
  }

  navigateToChat(tfgId: number, studentName: string, studentSurname: string, studentProfilePicture: string) {
    this.router.navigate(['/chat', tfgId], {
      state: { name: studentName, surname: studentSurname, profilePicture: studentProfilePicture }
    });
  }

  deleteTFG(tfgId: number) {
    this.tfgIdToDelete = tfgId;
    this.showConfirmModal = true;
  }

  confirmDelete() {
    if (this.tfgIdToDelete) {
      const token = localStorage.getItem('token');
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });

      this.http.delete(`${Environment.tfgApiUrl}/${this.tfgIdToDelete}`, { headers }).subscribe(
        () => {
          this.showConfirmModal = false;
          this.router.navigate([this.router.url]);
        },
        (error) => {
          console.error('Error al eliminar el TFG:', error);
          this.showConfirmModal = false;
        }
      );
    }
  }

  sortTable(columns: (keyof TFG)[], direction: 'asc' | 'desc') {
    this.filteredTfgs.sort((a, b) => {
      for (const column of columns) {
        let first = a[column];
        let second = b[column];

        if (typeof first === 'string' && typeof second === 'string') {
          first = first.toLowerCase();
          second = second.toLowerCase();
        }

        if (first < second) {
          return direction === 'asc' ? -1 : 1;
        } else if (first > second) {
          return direction === 'asc' ? 1 : -1;
        }
      }
      return 0;
    });
  }

  filterTfgs(): void {
    const searchNameLower = this.searchName.toLowerCase();
    const searchTFGLower = this.searchTFG.toLowerCase();

    this.filteredTfgs = this.tfgs.filter(tfg => {
      const matchesName = tfg.studentName.toLowerCase().includes(searchNameLower) ||
        tfg.studentSurname.toLowerCase().includes(searchNameLower);
      const matchesTFG = tfg.name.toLowerCase().includes(searchTFGLower);

      return matchesName && matchesTFG;
    });
  }

  closeModal() {
    this.showConfirmModal = false;
    this.tfgIdToDelete = null;
  }

  editTFG(tfg: any) {
    this.router.navigate(['/edición-TFG', tfg.id], { state: { tfg } });
  }
}


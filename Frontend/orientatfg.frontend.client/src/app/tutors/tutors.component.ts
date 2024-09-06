import { Router } from '@angular/router';
import { HostListener, Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Environment } from '../../environment';

interface Tutor {
  id: number;
  name: string;
  surname: string;
  profilePicture: string;
  departmentName: string;
  email: string;
  tfGs: string[];
}

interface Department {
  id: number;
  name: string;
}

@Component({
  selector: 'app-tutors',
  templateUrl: './tutors.component.html',
  styleUrls: ['./tutors.component.css', '../app.component.css']
})
export class TutorsComponent implements OnInit {
  isLoading = true;
  tutors: Tutor[] = [];
  filteredTutors: Tutor[] = [];
  searchName: string = ''; 
  searchEmail: string = ''; 
  searchTFG: string = ''; 
  departments: Department[] = [];
  selectedDepartments: string[] = [];
  dropdownOpen = false;


  constructor(private router: Router, private http: HttpClient) { }

  ngOnInit() {
    const token = localStorage.getItem('token');
    const id = localStorage.getItem('id');

    if (token && id) {
      this.getTutorsData(id, token);
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
        console.error('Get Departments error:', error);
      }
    );
  }

  getTutorsData(id: string, token: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.http.get<Tutor[]>(`${Environment.userApiUrl}/tutors`, { headers }).subscribe(
      (data) => {
        this.tutors = data;
        this.filteredTutors = data;
        this.getDepartments(token);
      },
      (error) => {
        console.error('Get Tutors error:', error);
      }
    );
  }

  toggleDepartment(departmentName: string): void {
    const index = this.selectedDepartments.indexOf(departmentName);
    if (index === -1) {
      this.selectedDepartments.push(departmentName);
    } else {
      this.selectedDepartments.splice(index, 1);
    }
    this.filterTutors();
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  filterTutors(): void {
    const searchNameLower = this.searchName.toLowerCase();
    const searchEmailLower = this.searchEmail.toLowerCase();
    const searchTFGLower = this.searchTFG.toLowerCase();

    this.filteredTutors = this.tutors.filter(tutor => {
      const matchesName = tutor.name.toLowerCase().includes(searchNameLower) ||
        tutor.surname.toLowerCase().includes(searchNameLower);
      const matchesEmail = tutor.email.toLowerCase().includes(searchEmailLower);
      const matchesDepartment = this.selectedDepartments.length === 0 ||
        this.selectedDepartments.includes(tutor.departmentName);
      const matchesTFG = this.searchTFG === '' ||
        tutor.tfGs.some(tfg => tfg.toLowerCase().includes(searchTFGLower));

      return matchesName && matchesEmail && matchesDepartment && matchesTFG;
    });
  }

  sortTable(columns: (keyof Tutor)[], direction: 'asc' | 'desc') {
    this.filteredTutors.sort((a, b) => {
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

  navigateToNewTutor() {
    this.router.navigate(['/registro-tutor']);
  }
}


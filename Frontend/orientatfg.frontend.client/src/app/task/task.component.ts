import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, HostListener, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Environment } from '../../environment';
import { Location } from '@angular/common';

enum StatusEnum {
  Pendiente = 1,
  Realizado = 2
}

interface TaskUpdate {
  id: number;
  totalHours?: number | null;
  statusId: number;
  order: number;
}

interface UpdateTaskPanel {
  mainTaskId: number;
  newSubTasks?: SubTask[];
  updatedSubTasks?: TaskUpdate[];
  deletedSubTasksIds?: number[];
  obtainedPoints?: number;
}

interface MainTask {
  id: number;
  name: string;
  description: string;
  estimatedHours: number;
  totalHours: number;
  status: string;
  order: number;
  createdBy: string;
  deadline: string;
  maximumPoints: number;
  obtainedPoints: number;
  commentsCount: number | null;
  subTasks: SubTask[];
}

interface SubTask {
  id: number | null;
  name: string;
  estimatedHours: number | null;
  totalHours: number | null;
  status: string;
  order: number;
  createdBy: string;
  commentsCount: number | null;
}

@Component({
  selector: 'app-task',
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.css', '../app.component.css']
})
export class TaskComponent implements OnInit {
  tfgName: string = '';
  isLoading = true;
  mainTask: MainTask = {
    id: 0,
    name: '',
    description: '',
    estimatedHours: 0,
    totalHours: 0,
    status: '',
    order: 0,
    createdBy: '',
    deadline: '',
    maximumPoints: 0,
    obtainedPoints: 0,
    commentsCount: 0,
    subTasks: []
  };


  totalInvertido: number = 0;

  pendientesTasks: SubTask[] = [];
  realizadasTasks: SubTask[] = [];
  newSubTasks: SubTask[] = [];
  tasksToUpdate: TaskUpdate[] = [];
  originalSubTasks: TaskUpdate[] = [];
  deletedSubTasks: number[] = [];
  invalidTasks: Set<number> = new Set();
  invalidFields: Map<number, { name: boolean; estimatedHours: boolean }> = new Map();

  hasPendingChanges: boolean = false;
  role: number = 0;
  showConfirmModal: boolean = false;
  subtaskToDelete: SubTask | null = null;
  tempObtainedPoints: number = 0;
  hasError: boolean = false;
  errorMessage = '';
  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient, private location: Location) { }

  ngOnInit() {
    const token = localStorage.getItem('token');
    const taskId = this.route.snapshot.paramMap.get('id');
    const roleString = localStorage.getItem('role');
    this.role = roleString ? +roleString : 0;

    if (token && taskId) {
      this.isLoading = true;
      this.getMainTask(token, taskId);
    }
  }

  getMainTask(token: string, taskId: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.http.get<MainTask>(`${Environment.tfgApiUrl}/main-task/${taskId}`, { headers }).subscribe(
      (data) => {
        this.mainTask = data;
        this.isLoading = false;

        this.pendientesTasks = data.subTasks.filter(subTask => subTask.status === 'Pendiente');
        this.realizadasTasks = data.subTasks.filter(subTask => subTask.status === 'Realizado');

        this.tasksToUpdate = data.subTasks.map(task => ({
          id: task.id!,
          totalHours: task.totalHours,
          statusId: task.status === 'Pendiente' ? StatusEnum.Pendiente : StatusEnum.Realizado,
          order: task.order
        }));

        this.originalSubTasks = data.subTasks.map(task => ({
          id: task.id!,
          totalHours: task.totalHours,
          statusId: task.status === 'Pendiente' ? StatusEnum.Pendiente : StatusEnum.Realizado,
          order: task.order
        }));
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

  getModifiedTasks(): TaskUpdate[] {
    return this.tasksToUpdate.filter((updatedTask) => {
      const originalTask = this.originalSubTasks.find(task => task.id === updatedTask.id);
      return originalTask && (
        originalTask.totalHours !== updatedTask.totalHours ||
        originalTask.statusId !== updatedTask.statusId ||
        originalTask.order !== updatedTask.order
      );
    });
  }

  getTotalEstimatedHours(): number {
    return this.pendientesTasks.reduce((total, task) => total + (task.estimatedHours ?? 0), 0);
  }

  getTotalHours(): number {
    return this.realizadasTasks.reduce((total, task) => total + task.totalHours!, 0);
  }

  getProgressPercentage(): number {
    const totalEstimatedHours = this.mainTask.subTasks.reduce((total, task) => total + (task.estimatedHours ?? 0), 0);
    const completedEstimatedHours = this.realizadasTasks.reduce((total, task) => total + task.estimatedHours!, 0);
    return totalEstimatedHours === 0 ? 0 : Math.round((completedEstimatedHours / totalEstimatedHours) * 100);
  }

  updateSubTaskData(task: SubTask) {
    this.hasPendingChanges = true;

    if (task.status === 'Pendiente') {
      this.realizadasTasks = this.realizadasTasks.filter(t => t.id === null ? t !== task : t.id !== task.id);
      if (!this.pendientesTasks.some(t => t.id === null ? t === task : t.id === task.id)) {
        this.pendientesTasks.push(task);
      }
    } else if (task.status === 'Realizado') {
      this.pendientesTasks = this.pendientesTasks.filter(t => t.id === null ? t !== task : t.id !== task.id);
      if (!this.realizadasTasks.some(t => t.id === null ? t === task : t.id === task.id)) {
        this.realizadasTasks.push(task);
      }
    }

    this.sortSubTasks();

    if (task.id) {
      const existingUpdateIndex = this.tasksToUpdate.findIndex(t => t.id === task.id);

      if (existingUpdateIndex !== -1) {
        this.tasksToUpdate[existingUpdateIndex].totalHours = task.totalHours;
        this.tasksToUpdate[existingUpdateIndex].statusId = task.status === 'Pendiente' ? StatusEnum.Pendiente : StatusEnum.Realizado;
        this.tasksToUpdate[existingUpdateIndex].order = task.order;
      }
    }
  }

  sortSubTasks() {
    this.pendientesTasks.sort((a, b) => a.order - b.order);
    this.realizadasTasks.sort((a, b) => a.order - b.order);
  }

  addNewSubTask() {
    this.hasPendingChanges = true;

    const role = localStorage.getItem('role');
    let createdBy = 'User';
    if (role === '1') {
      createdBy = 'Estudiante';
    } else if (role === '2') {
      createdBy = 'Tutor';
    }

    const newOrder = this.pendientesTasks.length + this.realizadasTasks.length + this.newSubTasks.length + 1;
    const newSubTask = {
      id: null,
      order: newOrder,
      name: '',
      createdBy: createdBy,
      estimatedHours: null,
      totalHours: null,
      statusId: StatusEnum.Pendiente,
      status: 'Pendiente',
      commentsCount: null,
    };
    this.newSubTasks.push(newSubTask);
  }

  saveAllChanges() {
    this.validateTasks();

    if (this.invalidTasks.size > 0) {
      alert('Hay tareas con campos obligatorios por rellenar.');
      return;
    }

    const token = localStorage.getItem('token');
    if (token) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });

      const modifiedTasks = this.getModifiedTasks();
      const body = {
        updatedSubTasks: modifiedTasks,
        newSubTasks: this.newSubTasks,
        mainTaskId: this.route.snapshot.paramMap.get('id'),
        deletedSubTasksIds: this.deletedSubTasks,
        ...(this.tempObtainedPoints !== 0 && { obtainedPoints: this.tempObtainedPoints })
      };

      this.http.post(`${Environment.tfgApiUrl}/panel`, body, { headers }).subscribe(
        () => {
          window.location.reload();
        },
        (error) => {
          console.error('Error saving task data:', error);
        }
      );
    }
  }

  validateTasks() {
    this.invalidTasks.clear();
    this.invalidFields.clear();

    this.newSubTasks.forEach(task => {
      const isNameInvalid = !task.name;
      const isEstimatedHoursInvalid = task.estimatedHours === null;

      if (isNameInvalid || isEstimatedHoursInvalid) {
        this.invalidTasks.add(task.order);
        this.invalidFields.set(task.order, {
          name: isNameInvalid,
          estimatedHours: isEstimatedHoursInvalid
        });
      }
    });
  }

  isTaskInvalid(task: SubTask): boolean {
    return this.invalidTasks.has(task.order);
  }

  isFieldInvalid(task: SubTask, field: 'name' | 'estimatedHours'): boolean {
    const fields = this.invalidFields.get(task.order);
    return fields ? fields[field] : false;
  }

  goBack() {
    if (this.hasPendingChanges) {
      const confirmLeave = confirm('Tiene cambios sin guardar, ¿seguro que desea salir?');
      if (confirmLeave) {
        this.location.back();
      }
      return;
    }
    this.location.back();
  }

  closeModal() {
    this.showConfirmModal = false;
    this.subtaskToDelete = null;
  }

  confirmDelete() {
    if (this.subtaskToDelete != null) {
      let taskList: SubTask[] = [];
      let newTaskList: SubTask[] = [];
      let taskIndex: number;
      let orderToAdjust: number | null = null;

      if ('id' in this.subtaskToDelete) {
        taskList = this.subtaskToDelete.status === 'Pendiente' ? this.pendientesTasks : this.realizadasTasks;
        taskIndex = taskList.findIndex(t => t.id === this.subtaskToDelete!.id);

        if (taskIndex !== -1) {
          orderToAdjust = taskList[taskIndex].order;
          taskList.splice(taskIndex, 1);
          this.deletedSubTasks.push(this.subtaskToDelete.id!);
        }


      } else {
        newTaskList = this.newSubTasks;
        taskIndex = newTaskList.findIndex(t => t === this.subtaskToDelete);

        if (taskIndex !== -1) {
          orderToAdjust = newTaskList[taskIndex].order;

          newTaskList.splice(taskIndex, 1);
        }
      }

      if (orderToAdjust !== null) {
        this.updateOrderNumbers(orderToAdjust);
      }

      this.hasPendingChanges = true;
    }
  }

  deleteSubTask(task: SubTask) {
    this.subtaskToDelete = task;
    this.showConfirmModal = true;
  }

  updateOrderNumbers(orderToAdjust: number) {
    this.pendientesTasks.forEach(task => {
      if (task.order > orderToAdjust) {
        task.order = task.order - 1;
      }
    });

    this.realizadasTasks.forEach(task => {
      if (task.order > orderToAdjust) {
        task.order = task.order - 1;
      }
    });

    this.newSubTasks.forEach(task => {
      if (task.order > orderToAdjust) {
        task.order = task.order - 1;
      }
    });

    this.tasksToUpdate.forEach(task => {
      if (task.order > orderToAdjust) {
        task.order = task.order - 1;
      }
    });
  }


  goToTaskDetail(task: any) {
    this.router.navigate(['/detalle-subtarea', task.id]);
  }

  goToMainTaskDetail(task: any) {
    this.router.navigate(['/detalle-tarea', task.id]);
  }
}






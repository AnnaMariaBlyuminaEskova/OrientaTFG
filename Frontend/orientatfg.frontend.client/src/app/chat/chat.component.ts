import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Environment } from '../../environment';
import { Location } from '@angular/common';
import { ChatService } from './chat.service';

interface User {
  name: string;
  surname: string;
  profilePicture: string;
  isUserOnline: boolean;
}

interface Message {
  text: string;
  createdBy: string;
  createdOn: Date;
}

interface DateGroup {
  date: Date;
  messages: Message[];
}

enum RoleEnum {
  Estudiante = 1,
  Tutor = 2
}

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css', '../app.component.css']
})
export class ChatComponent implements OnInit, OnDestroy {
  newMessage: string = '';
  messages: Message[] = [];
  groupedMessages: DateGroup[] = [];
  isLoading: boolean = true;
  userRole: string = '';
  tfgId: number | null = null;
  oldestMessageTimestamp: Date | null = null;
  isFetchingMoreMessages: boolean = false;
  limit: number = 3;
  moreMessages: boolean = true;
  chatUser: User | null = null;
  hasError: boolean = false;
  errorMessage = '';

  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient, private location: Location, private chatService: ChatService) { }

  ngOnInit() {
    const role = localStorage.getItem('role');
    if (role) {
      const parsedRole = parseInt(role);

      if (parsedRole == RoleEnum.Estudiante) {
        this.userRole = 'Estudiante';
        this.getTutorData();
      }
      else {
        this.userRole = 'Tutor';
        const state = history.state;
        if (state) {
          this.chatUser = {
            name: state.name,
            surname: state.surname,
            profilePicture: state.profilePicture,
            isUserOnline: false
          }
        }
      }
    }
    this.route.paramMap.subscribe(params => {
      const tfgIdParam = params.get('tfgId');
      this.tfgId = tfgIdParam ? parseInt(tfgIdParam, 10) : null;
      if (this.tfgId !== null) {
        this.getMessageHistory();
        this.startChatService(this.tfgId);
      }
    });
  }

  async ngOnDestroy() {
    if (this.tfgId !== null) {
      await this.chatService.leaveChat(this.tfgId, this.userRole);
    }
  }

  startChatService(tfgId: number): void {
    this.chatService.startConnection(localStorage.getItem('token') || '')
      .then(() => {
        return this.chatService.joinChat(tfgId, this.userRole);
      })
      .then(() => {
        this.chatService.receiveMessage();

        this.chatService.onMessageReceived.subscribe((data: { user: string, message: string, timestamp: Date }) => {
          this.onReceiveMessage(data.user, data.message, data.timestamp);
        });

        this.chatService.onUserStatusChanged.subscribe((user: { role: string, isOnline: boolean }) => {
          if (this.chatUser && this.userRole !== user.role) {
            this.chatUser.isUserOnline = user.isOnline;
          }
        });
      })
      .catch((err:any) => {
        console.error('Error al iniciar el servicio de chat o unirse al chat:', err);
      });
  }

  getMessageHistory() {
    const token = localStorage.getItem('token');
    if (token && this.tfgId) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });

      let url = `${Environment.tfgApiUrl}/messages/${this.tfgId}?limit=${this.limit}`;
      if (this.oldestMessageTimestamp) {

        url += `&beforeTimestamp=${this.oldestMessageTimestamp}`;
      }

      this.http.get<Message[]>(url, { headers }).subscribe(
        (data) => {
          if (data.length > 0) {
            this.oldestMessageTimestamp = data[data.length - 1].createdOn;
            this.messages = [...data.reverse(), ...this.messages];
            this.groupMessagesByDate();
          }
          if (data.length < this.limit) {
            this.moreMessages = false;
          }
          this.isLoading = false;
          this.isFetchingMoreMessages = false;
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

  getTutorData() {
    const token = localStorage.getItem('token');
    const id = localStorage.getItem('id');
    if (token && id) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });

      let url = `${Environment.userApiUrl}/student-tutor/${id}`;

      this.http.get<User>(url, { headers }).subscribe(
        (data) => {
          this.chatUser = data;
          this.chatUser.isUserOnline = false;
        },
        (error) => {
          console.error('Error al obtener la información del tutor:', error);
        }
      );
    }
  }

  loadMoreMessages() {
    if (!this.isFetchingMoreMessages) {
      this.isFetchingMoreMessages = true;
      this.getMessageHistory();
    }
  }

  groupMessagesByDate() {
    const groups: { [key: string]: Message[] } = {};

    this.messages.forEach(message => {
      const date = new Date(message.createdOn).toDateString();
      if (!groups[date]) {
        groups[date] = [];
      }
      groups[date].push(message);
    });

    this.groupedMessages = Object.keys(groups).map(date => {
      return {
        date: new Date(date),
        messages: groups[date]
      };
    });
  }

  formatDate(date: Date): string {
    const days = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

    const dayName = days[date.getDay()];
    const day = date.getDate();
    const month = months[date.getMonth()];
    const year = date.getFullYear();

    return `<strong>${dayName} ${day} de ${month}</strong> (${this.pad(date.getDate())}/${this.pad(date.getMonth() + 1)}/${year})`;
  }

  pad(number: number): string {
    return number < 10 ? '0' + number : number.toString();
  }

  submitMessage() {
    const token = localStorage.getItem('token');
    if (token) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });

      const body = JSON.stringify(this.newMessage);

      this.http.post(`${Environment.tfgApiUrl}/messages/${this.tfgId}`, body, { headers }).subscribe(
        (data) => {
          if (this.tfgId) {
            this.chatService.sendMessage(this.tfgId, this.newMessage, this.userRole);
            this.newMessage = '';
          }
        },
        (error) => {
          console.error('Error al enviar un mensaje:', error);
        }
      );
    }
  }

  onReceiveMessage(user: string, message: string, timestamp: Date) {
    console.log(`Mensaje recibido - Usuario: ${user}, Mensaje: ${message}, Fecha: ${timestamp}`);
    const newMessage: Message = {
      createdBy: user,
      createdOn: new Date(timestamp),
      text: message
    };
    this.messages.push(newMessage);
    this.groupMessagesByDate();
  }

  async goBack() {
    if (this.tfgId !== null) {
      await this.chatService.leaveChat(this.tfgId, this.userRole);
    }
    this.location.back();
  }
}






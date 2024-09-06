import { Injectable, EventEmitter } from '@angular/core';
import { Environment } from '../../environment';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private hubConnection: signalR.HubConnection | undefined;
  public onMessageReceived: EventEmitter<{ user: string, message: string, timestamp: Date }> = new EventEmitter();
  public onUserStatusChanged: EventEmitter<{ role: string, isOnline: boolean }> = new EventEmitter();

  constructor() {
  }

  startConnection(token: string): Promise<void> {
    if (!this.hubConnection) {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${Environment.tfgApiUrl}/chatHub`, {
          accessTokenFactory: () => token
        })
        .build();

      return this.hubConnection.start()
        .then(() => {
          console.log('Conexión SignalR iniciada');
          this.listenToUserStatusChanges();
        })
        .catch((err: any) => {
          console.error('Error al iniciar conexión SignalR: ', err);
          throw err; 
        });
    } else {
      return Promise.resolve();
    }
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => console.log('Conexión SignalR finalizada'));
      this.hubConnection = undefined;
    }
  }

  joinChat(tfgId: number, sender: string): void {
    if (this.hubConnection) {
      this.hubConnection.invoke('JoinChat', tfgId, sender)
        .catch((err: any) => console.error('Error al enviar el mensaje:', err));
    }
  }

  leaveChat(tfgId: number, sender: string): void {
    if (this.hubConnection) {
      this.hubConnection.invoke('LeaveChat', tfgId, sender)
        .catch((err: any) => console.error('Error al enviar el mensaje:', err));
    }
  }

  receiveMessage() {
    if (this.hubConnection) {
      this.hubConnection.on('ReceiveMessage', (message: string, sender: string, timestamp: Date) => {
        console.log(`Mensaje recibido - Usuario: ${sender}, Mensaje: ${message}, Fecha: ${timestamp}`);
        this.onMessageReceived.emit({ user: sender, message, timestamp });
      });
    }
  }

  sendMessage(tfgId: number, message: string, sender: string): void {
    if (this.hubConnection) {
      this.hubConnection.invoke('SendMessage', tfgId, message, sender)
        .catch((err: any) => console.error('Error al enviar el mensaje:', err));
    }
  }

  private listenToUserStatusChanges(): void {
    if (this.hubConnection) {
      this.hubConnection.on('UserStatusChanged', (role: string, isOnline: boolean) => {
        this.onUserStatusChanged.emit({ role, isOnline });
      });
    }
  }
}








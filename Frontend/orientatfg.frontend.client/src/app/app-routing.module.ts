import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { TFGsComponent } from './tfgs/tfgs.component';
import { NewTFGComponent } from './new-tfg/new-tfg.component';
import { PanelTFGComponent } from './panel-tfg/panel-tfg.component';
import { NewTaskComponent } from './new-task/new-task.component';
import { TaskComponent } from './task/task.component';
import { RegisterComponent } from './register/register.component';
import { TutorsComponent } from './tutors/tutors.component';
import { RegisterTutorComponent } from './register-tutor/register-tutor.component';
import { ProfileComponent } from './profile/profile.component';
import { TaskDetailComponent } from './task-detail/task-detail.component';
import { ChatComponent } from './chat/chat.component';
import { NewPanelComponent } from './new-panel/new-panel.component';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'nueva-tarea', component: NewTaskComponent },
  { path: 'edición-tarea/:id', component: NewTaskComponent },
  { path: 'nuevo-TFG', component: NewTFGComponent },
  { path: 'edición-TFG/:id', component: NewTFGComponent },
  { path: 'panel/:id', component: PanelTFGComponent },
  { path: 'panel', component: NewPanelComponent },
  { path: 'tarea/:id', component: TaskComponent },
  { path: 'TFGs', component: TFGsComponent },
  { path: 'registro', component: RegisterComponent },
  { path: 'tutores', component: TutorsComponent },
  { path: 'registro-tutor', component: RegisterTutorComponent },
  { path: 'perfil', component: ProfileComponent },
  { path: 'detalle-tarea/:id', component: TaskDetailComponent },
  { path: 'detalle-subtarea/:id', component: TaskDetailComponent },
  { path: 'chat/:tfgId', component: ChatComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

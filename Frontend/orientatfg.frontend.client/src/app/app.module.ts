import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { IgxSelectModule } from 'igniteui-angular';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';
import { TFGsComponent } from './tfgs/tfgs.component';
import { LoginComponent } from './login/login.component';
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
import { ErrorCardComponent } from './error-card/error-card.component';
import { NewPanelComponent } from './new-panel/new-panel.component';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    LoginComponent,
    TFGsComponent,
    NewTFGComponent,
    PanelTFGComponent,
    NewTaskComponent,
    TaskComponent,
    RegisterComponent,
    TutorsComponent,
    RegisterTutorComponent,
    ProfileComponent,
    TaskDetailComponent,
    ChatComponent,
    ErrorCardComponent,
    NewPanelComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    BrowserAnimationsModule,
    IgxSelectModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}

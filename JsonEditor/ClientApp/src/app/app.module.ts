import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FillJsonComponent } from './fill-json/fill-json.component';

import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatTreeModule } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/';

// import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

import { MainMenuComponent } from './main-menu/main-menu.component';
import { QuestionsAnswersComponent } from './questions-answers/questions-answers.component';
import { LocationsComponent } from './locations/locations.component';
import { CoursesComponent } from './courses/courses.component';

import { AddNewMenuDialog } from './add-new-menu/add-new-menu.component';
import { NewQADialog } from './new-qadialog/new-qadialog.component';
import { NewCourseDialog } from './new-course-dialog/new-course-dialog.component';



@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    FillJsonComponent,
    FetchDataComponent,
    MainMenuComponent,
    QuestionsAnswersComponent,
    LocationsComponent,
    CoursesComponent,

    AddNewMenuDialog,
    NewQADialog,
    NewCourseDialog,
    
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,

    FormsModule,
    ReactiveFormsModule,

    // material
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatChipsModule,
    MatTableModule,
    MatTreeModule,
    MatIconModule,
    MatDialogModule,
    MatDatepickerModule,
    MatNativeDateModule,

    //FontAwesomeModule,

    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'main-menu', component: MainMenuComponent },
      { path: 'questions-answers', component: QuestionsAnswersComponent },
      { path: 'locations', component: LocationsComponent },
      { path: 'courses', component: CoursesComponent },
      { path: 'fill-json', component: FillJsonComponent },
      { path: 'fetch-data', component: FetchDataComponent },
    ]),
    BrowserAnimationsModule
  ],
  providers: [],
  bootstrap: [AppComponent],
  entryComponents: [AddNewMenuDialog, NewQADialog, NewCourseDialog],
})
export class AppModule { }

import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatDialog } from '@angular/material';
import { NewCourseDialog } from '../new-course-dialog/new-course-dialog.component';



export interface Course {
  name: string;
  resources: string;
  startDate: Date;
  registrationStartDate: Date;
}

export interface ICourseModel {
  lang: string;
  courses: Course[];
}

@Component({
  selector: 'app-courses',
  templateUrl: './courses.component.html',
  styleUrls: ['./courses.component.css']
})
export class CoursesComponent {

  courses: ICourseModel[];

  baseUrl: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private dialog: MatDialog) {

    this.baseUrl = baseUrl + 'api/Courses';

    http.get<ICourseModel[]>(this.baseUrl)
      .subscribe(result => {        
        this.courses = result;
      },
        error => console.error(error));
  }


  editField: string;

  changeValue(event: any) {
    this.editField = event.target.textContent;
  }

  updateData(i: number, j: number, property: string, event: any) {

    const editField = event.target.textContent;

    this.courses[i].courses[j][property] = editField.trim();

    this.sendPut(i, j);

  }

  delete(i: number, j: number) {
    this.courses[i].courses.splice(j, 1);
    this.sendPut(i, j);
  }




  sendPut(i: number, j: number) {
    let url = this.baseUrl + '/' + i + '/' + j;
    let body = JSON.stringify(this.courses[i].courses[j]);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    return this.http.put(url, body, httpOptions)
      .subscribe();
  }


  openNewCourseDialog() {
    const dialogRef = this.dialog.open(NewCourseDialog, {
      width: '60%'
    });

    dialogRef.afterClosed().subscribe(() => {
      this.http.get<ICourseModel[]>(this.baseUrl)
        .subscribe(result => {
          this.courses = result;
        },
          error => console.error(error)
        );
    })

  }

}

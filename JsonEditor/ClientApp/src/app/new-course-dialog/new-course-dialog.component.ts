import { Component, OnInit, Inject } from '@angular/core';
import { FormGroup, FormBuilder, FormArray } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatDialogRef } from '@angular/material';
import { ILang } from '../add-new-menu/add-new-menu.component';

@Component({
  selector: 'app-new-course-dialog',
  templateUrl: './new-course-dialog.component.html',
  styleUrls: ['./new-course-dialog.component.css']
})
export class NewCourseDialog implements OnInit {

  baseUrl: string;
  courseForm: FormGroup;

  languages: ILang[] = [
    { value: 'uk-ua', viewValue: 'uk-ua' },
    { value: 'en-gb', viewValue: 'en-gb' }
  ];

  constructor(private fb: FormBuilder,
              private http: HttpClient,
              @Inject('BASE_URL') baseUrl: string,
              public dialogRef: MatDialogRef<NewCourseDialog>
            ) {
              this.baseUrl = baseUrl + '/api/Courses'
            }

  ngOnInit() {
    this.courseForm = this.fb.group({
      lang: "",
      courses: this.fb.array([])
    })
  }

  get coursesInnerArray() { return this.courseForm.get('courses') as FormArray }


  addCourseData() {
    const course = this.fb.group({
      name: "",
      resources: ""
    })

    this.coursesInnerArray.push(course);
  }

  deleteCourseData(i: number) {
    this.coursesInnerArray.removeAt(i);
  }

  Submit() {
    let body = JSON.stringify(this.courseForm.value);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    this.courseForm.reset();
    this.onNoClick();

    return this.http.post(this.baseUrl, body, httpOptions)
    .subscribe();

  }

  onNoClick(): void {
    this.dialogRef.close();
  }



}

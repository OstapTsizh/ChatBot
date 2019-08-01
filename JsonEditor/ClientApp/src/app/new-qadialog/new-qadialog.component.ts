import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatDialogRef } from '@angular/material';
import { FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';

export interface ILang {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-new-qadialog',
  templateUrl: './new-qadialog.component.html',
  styleUrls: ['./new-qadialog.component.css']
})
export class NewQADialog implements OnInit {

  baseUrl: string;
  qaForm: FormGroup;

  languages: ILang[] = [
    { value: 'uk-ua', viewValue: 'uk-ua' },
    { value: 'en-gb', viewValue: 'en-gb' }
  ];

  constructor(private fb: FormBuilder,
              private http: HttpClient,
              @Inject('BASE_URL') baseUrl: string,
              public dialogRef: MatDialogRef<NewQADialog>
            ) {
              this.baseUrl = baseUrl + '/api/QA'
            }

  ngOnInit() {
    this.qaForm = this.fb.group({
      lang: "",
      qAs: this.fb.array([])
    });    
  }

  get qaArray() { return this.qaForm.get('qAs') as FormArray }


  addQA() {
    const qa = this.fb.group({
      question: "",
      answer: this.fb.array([])
    })

    this.qaArray.push(qa);
  }

  deleteQA(i:number) {
    this.qaArray.removeAt(i);
  }


  addQaAnswer(inputQaAnswer, index: number) {
    let answerArray = this.qaArray.at(index).get('answer') as FormArray;

    answerArray.push(this.fb.control(inputQaAnswer.value));
    inputQaAnswer.value = '';
  }

  deleteQaAnswer(qaIndex: number, answerIndex: number) {
    (this.qaArray.at(qaIndex).get('answer') as FormArray).removeAt(answerIndex);
  }


  Submit() {
    let body = JSON.stringify(this.qaForm.value);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    this.qaForm.reset();
    this.onNoClick();

    return this.http.post(this.baseUrl, body, httpOptions)
    .subscribe();

  }


  onNoClick(): void {
    this.dialogRef.close();
  }


}

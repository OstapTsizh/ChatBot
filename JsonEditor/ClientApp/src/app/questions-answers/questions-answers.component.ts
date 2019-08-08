import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatDialog } from '@angular/material';
import { NewQADialog } from '../new-qadialog/new-qadialog.component';



export interface QAInner {
  question: string;
  answer: string[];
}

export interface IQAs {
  lang: string;
  qAs: QAInner[];
}


@Component({
  selector: 'app-questions-answers',
  templateUrl: './questions-answers.component.html',
  styleUrls: ['./questions-answers.component.css']
})
export class QuestionsAnswersComponent {

  QAs: IQAs[];
  baseUrl: string;

  constructor(private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    private dialog: MatDialog) {
    this.baseUrl = baseUrl + 'api/QA';

    http.get<IQAs[]>(this.baseUrl)
      .subscribe(result => {
        this.QAs = result;
      },
        error => console.error(error)
      );

  }



  editField: string;

  updateList(lang: number, qaInner: number, property: string, event: any) {

    const editField = event.target.textContent;

    this.QAs[lang].qAs[qaInner][property] = editField.trim();

    this.sendPut();
  }


  updateAnswer(i: number, j: number, k: number, event: any) {

    const editField = event.target.textContent;

    this.QAs[i].qAs[j].answer[k] = editField.trim();

    this.sendPut();
  }



  changeValue(event: any) {
    this.editField = event.target.textContent;
  }


  deleteAnswer(i: number, j: number, k: number) {
    this.QAs[i].qAs[j].answer.splice(k, 1);
    this.sendPut();
  }

  delete(i: number, j: number) {
    this.QAs[i].qAs.splice(j, 1);
    this.sendPut();
  }

  sendPut() {
    let body = JSON.stringify(this.QAs);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    return this.http.put(this.baseUrl, body, httpOptions)
      .subscribe();
  }



  openNewQADialog() {
    const dialogRef = this.dialog.open(NewQADialog, {
      width: '60%'
    });

    dialogRef.afterClosed().subscribe(() => {
      this.http.get<IQAs[]>(this.baseUrl)
        .subscribe(result => {
          this.QAs = result;
        },
          error => console.error(error)
        );
    })




  }





}

import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatDialog } from '@angular/material';
import { AddNewMenuDialog } from '../add-new-menu/add-new-menu.component';


export interface IItem {
  name: string;
  dialog: string;
  resources: string[]
}

export interface IMainMenu {
  lang: string;
  items: IItem[]
}



@Component({
  selector: 'app-main-menu',
  templateUrl: './main-menu.component.html',
  styleUrls: ['./main-menu.component.css']
})
export class MainMenuComponent {

  baseUrl: string;

  public data: IMainMenu[];

  constructor(private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    public dialog: MatDialog) {

    this.baseUrl = baseUrl + '/api/MainMenu';

    http.get<IMainMenu[]>(this.baseUrl)
      .subscribe(result => {
        this.data = result;
      }, error => console.error(error));
  }

  editField: string;

  changeValue(event: any) {
    this.editField = event.target.textContent;
  }

  updateData(i: number, j: number, property: string, event: any) {
    const editField = event.target.textContent;

    if (property === 'resources') {
      var arr = this.editField.split(';,.!?');
      this.data[i].items[j][property] = arr;
    }
    else {
      this.data[i].items[j][property] = editField.trim();
    }

    this.sendPut();
  }

  delete(i: number, j: number) {
    this.data[i].items.splice(j, 1);
    this.sendPut();
  }


  sendPut() {
    let body = JSON.stringify(this.data);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    return this.http.put(this.baseUrl, body, httpOptions)
      .subscribe();

  }


  openNewMainMenuDialog(): void {
    const dialogRef = this.dialog.open(AddNewMenuDialog, {
      width: '60%'
    });

    dialogRef.afterClosed().subscribe(result => {
      this.http.get<IMainMenu[]>(this.baseUrl)
        .subscribe(result => {
          this.data = result;
        },
          error => console.error(error));
    });
  }


}
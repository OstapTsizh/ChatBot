import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';


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

  public data: IMainMenu[];

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<IMainMenu[]>(baseUrl + 'api/SampleData/MainMenu').subscribe(result => {
      this.data = result;
    }, error => console.error(error));
  }

  


}
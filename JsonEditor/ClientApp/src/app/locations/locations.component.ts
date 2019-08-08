import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';


export interface IModel {
  country: string;
  cities: string[];
}

export interface ILocation {
  lang: string;
  model: IModel;
}

@Component({
  selector: 'app-locations',
  templateUrl: './locations.component.html',
  styleUrls: ['./locations.component.css']
})
export class LocationsComponent {

  public locations: ILocation[];

  baseUrl: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {

    this.baseUrl = baseUrl + '/api/Locations';

    http.get<ILocation[]>(this.baseUrl)
      .subscribe(result => {
        this.locations = result;
      },
        error => console.error(error));
  }

  editField: string;

  updateList(i: number, property: string, event: any) {
    const editField = event.target.textContent;

    this.locations[i].model[property] = editField.trim();

    this.sendPut();
  }


  changeValue(event: any) {
    this.editField = event.target.textContent;
  }


  updateCity(i: number, j: number, event: any) {
    const editField = event.target.textContent;

    this.locations[i].model.cities[j] = editField.trim();

    this.sendPut();
  }


  sendPut() {
    let body = JSON.stringify(this.locations);
   
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    return this.http.put(this.baseUrl, body, httpOptions).subscribe();

  }

}

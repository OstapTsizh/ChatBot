import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class PlannedEventsHttpService {

  baseUrl: string;

  constructor(private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl + 'api/PlannedEvents';
  }

  getPlannedEvents()
  {
    return this.http.get(this.baseUrl);
  }

  sendPut(plannedEvents: any)
  {
    let body = JSON.stringify(plannedEvents);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };
    return this.http.put(this.baseUrl, body, httpOptions);
  }

  sendPost(eventForm: any)
  {
    let body = JSON.stringify(eventForm);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };
    return this.http.post(this.baseUrl, body, httpOptions);
  }

}

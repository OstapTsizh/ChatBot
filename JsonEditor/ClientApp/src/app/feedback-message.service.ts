import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class FeedbackMessageService {

  baseUrl: string;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl + '/api/Feedbacks';
  }

  getFeedbacks() {
    return this.http.get(this.baseUrl);
  }

  deleteFeedback(feedback) {
    let url = this.baseUrl + '/' + feedback.id;
    return this.http.delete(url);
  }

}

import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource, MatPaginator, MatSort } from '@angular/material';
import { FeedbackMessageService } from '../feedback-message.service';
import { HttpClient } from '@angular/common/http';

export interface IFeedback {
  id: number;
  message: string;
  date: Date;
}

@Component({
  selector: 'app-feedback',
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.css']
})
export class FeedbackComponent implements OnInit {

 
  isLoading = true;

  constructor(private feedbackService: FeedbackMessageService) { }

  displayedColumns: string[] = ['message', 'date', ' '];
  dataSource;
 
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  ngOnInit() {

    this.feedbackService.getFeedbacks().subscribe((result: IFeedback[]) => {
      this.dataSource = new MatTableDataSource(result);
      this.dataSource.sort = this.sort;
      this.dataSource.paginator = this.paginator;
      
      this.isLoading = false;
    })

    // If the user changes the sort order, reset back to the first page.
    this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);   

  }


  Delete(element: IFeedback)
  {
    this.isLoading = true;
    this.feedbackService.deleteFeedback(element).subscribe( () => {
      this.ngOnInit();
    })
    
  }


}



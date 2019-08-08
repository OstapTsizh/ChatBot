import { Component, OnInit, ViewChild } from '@angular/core';
import { UserquestionsService } from '../userquestions.service';
import { MatPaginator, MatSort, MatTableDataSource } from '@angular/material';


export interface IUserQuestion {
  id: number;
  message: string;
  date: Date;
}

@Component({
  selector: 'app-user-questions',
  templateUrl: './user-questions.component.html',
  styleUrls: ['./user-questions.component.css']
})
export class UserQuestionsComponent implements OnInit {

  isLoading: Boolean = true;

  constructor(private uqService: UserquestionsService) { }

  displayedColumns: string[] = ['message', 'date', '_'];
  dataSource: any;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  ngOnInit() {
    this.uqService.getUserQuestions().subscribe((result: IUserQuestion[]) => {
      this.dataSource = new MatTableDataSource(result);
      this.dataSource.sort = this.sort;
      this.dataSource.paginator = this.paginator;

      this.isLoading = false;
    })

    // If the user changes the sort order, reset back to the first page.
    this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);
  
  }

  Delete(element: IUserQuestion)
  {
    this.isLoading = true;
    this.uqService.deleteUserQuestion(element).subscribe(() => {
      this.ngOnInit();
    })
  }

}

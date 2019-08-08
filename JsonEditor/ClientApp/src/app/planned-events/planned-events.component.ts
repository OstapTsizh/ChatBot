import { Component, OnInit } from '@angular/core';
import { PlannedEventsHttpService } from '../planned-events-http-service.service';
import { MatDialog } from '@angular/material';
import { NewEventDialog } from '../new-event/new-event.component';

export interface Event {
  name: string,
  resources: string[]
}

export interface IPlannedEvents {
  lang: string,
  events: Event[]
}

@Component({
  selector: 'app-planned-events',
  templateUrl: './planned-events.component.html',
  styleUrls: ['./planned-events.component.css'],
  providers: [ PlannedEventsHttpService ]
})
export class PlannedEventsComponent implements OnInit {

  plannedEvents : IPlannedEvents[];

  constructor(private service: PlannedEventsHttpService,  
    private dialog: MatDialog) { }

  ngOnInit() {
    this.service.getPlannedEvents().subscribe((result: IPlannedEvents[]) =>
    {
      this.plannedEvents = result;
    })

  }


  editField: string;

  changeValue(event: any) {
    this.editField = event.target.textContent;
  }

  updateList(lang: number, qaInner: number, property: string, event: any) {

    const editField = event.target.textContent;

    this.plannedEvents[lang].events[qaInner][property] = editField.trim();

    this.sendPut();
  }

  updateResource(i: number, j: number, k: number, event: any) {

    const editField = event.target.textContent;

    this.plannedEvents[i].events[j].resources[k] = editField.trim();

    this.sendPut();
  }

  delete(i: number, j: number) {
    this.plannedEvents[i].events.splice(j, 1);
    this.sendPut();
  }

  deleteResource(lang:number, eventInner:number, k:number)
  {
    this.plannedEvents[lang].events[eventInner].resources.splice(k, 1);
    this.sendPut();
  }

  sendPut(){
    this.service.sendPut(this.plannedEvents).subscribe();
  }

  openNewEventDialog() {
    const dialogRef = this.dialog.open(NewEventDialog, {
      width: '60%'
    });

    dialogRef.afterClosed().subscribe(() => {
      this.ngOnInit();
    }
    
    )
  }



}

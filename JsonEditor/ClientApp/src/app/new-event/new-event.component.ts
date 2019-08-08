import { Component, OnInit } from '@angular/core';
import { ILang } from '../add-new-menu/add-new-menu.component';
import { FormGroup, FormBuilder, FormArray } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { PlannedEventsHttpService } from '../planned-events-http-service.service';
import { MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-new-event',
  templateUrl: './new-event.component.html',
  styleUrls: ['./new-event.component.css']
})
export class NewEventDialog implements OnInit {

  eventForm: FormGroup;


  languages: ILang[] = [
    { value: 'uk-ua', viewValue: 'uk-ua' },
    { value: 'en-us', viewValue: 'en-us' }
  ];

  constructor(private fb: FormBuilder,
    private service: PlannedEventsHttpService,
    public dialogRef: MatDialogRef<NewEventDialog>
  ) {

  }

  ngOnInit() {
    this.eventForm = this.fb.group({
      lang: "",
      events: this.fb.array([])
    })
  }


  get eventsArray() { return this.eventForm.get('events') as FormArray }

  addEventData() {
    const event = this.fb.group({
      name: "",
      resources: this.fb.array([])
    })
    this.eventsArray.push(event);
  }

  addEventResource(inputItemResource, index: number) {
    let itemsResourcesArray = this.eventsArray.at(index).get('resources') as FormArray;

    itemsResourcesArray.push(this.fb.control(inputItemResource.value));
    inputItemResource.value = '';
  }

  deleteEventData(i)
  {
    this.eventsArray.removeAt(i);
  }

  Submit() {
    this.service.sendPost(this.eventForm.value).subscribe();
    this.eventForm.reset();
    this.onNoClick();
  }

  onNoClick(): void {
    this.dialogRef.close();
  }



}

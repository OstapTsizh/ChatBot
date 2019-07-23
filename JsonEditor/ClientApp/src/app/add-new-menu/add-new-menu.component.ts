import { Component, OnInit, Inject } from '@angular/core';
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';


export interface ILang {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'add-new-menu',
  templateUrl: './add-new-menu.component.html',
  styleUrls: ['./add-new-menu.component.css']
})
export class AddNewMenuComponent implements OnInit {

  languages: ILang[] = [
    {value: 'uk-ua', viewValue: 'uk-ua'},
    {value: 'en-us', viewValue: 'en-us'}
  ];

  constructor(private fb: FormBuilder, private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
   }

  baseUrl: string;
  MainMenuForm: FormGroup;

  ngOnInit() {

    this.MainMenuForm = this.fb.group({
      lang: "",
      items: this.fb.array([])
    })
    
  }


  get itemsArray() { return this.MainMenuForm.get('items') as FormArray }


  addItem() {
    const item = this.fb.group({
      name: "",
      dialog: "",
      resources: this.fb.array([])
    })

    this.itemsArray.push(item);
  }

  deleteItem(i) {
    this.itemsArray.removeAt(i);
  }


  

  addItemResource(inputItemResource, index: number)
  {     
    let itemsResourcesArray = this.itemsArray.at(index).get('resources') as FormArray;

    itemsResourcesArray.push(this.fb.control(inputItemResource.value));
    inputItemResource.value = '';
  }

  deleteItemResource(itemInArray: number, resourceIndex: number)
  {    
    (this.itemsArray.at(itemInArray).get('resources') as FormArray).removeAt(resourceIndex);
  }



  Submit() {
    let body = JSON.stringify(this.MainMenuForm.value);
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    

    this.MainMenuForm.get('items').reset();
    this.MainMenuForm.reset();

    return this.http.post(this.baseUrl + 'api/SampleData/MainMenu', body, httpOptions)
      .subscribe( result => console.log(result));
  }


//export interface

}

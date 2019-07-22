import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';

@Component({
  selector: 'app-fill-json',
  templateUrl: './fill-json.component.html',
  styleUrls: ['./fill-json.component.css']
})
export class FillJsonComponent implements OnInit {

  constructor(private fb: FormBuilder) { }

  JsonForm: FormGroup;
  
  ngOnInit() {

    

    let _model = this.fb.group({
      name: '',
      keywords: this.fb.array([]),
      questions: this.fb.array([]),
      decisions: this.fb.array([]),
    })


    this.JsonForm = this.fb.group({
      topic: '',
      model: _model
    })

  }

  get modelKeywords() { return this.JsonForm.get('model').get('keywords') as FormArray }

  addModelKeyword(word: string)
  {     
      this.modelKeywords.push(this.fb.control(word));
  }

  deleteModelKeyword(i)
  {    
    this.modelKeywords.removeAt(i);
  }
  
  get questionsForms() { return this.JsonForm.get('model').get('questions') as FormArray }
  get decisionsForms() { return this.JsonForm.get('model').get('decisions') as FormArray }

  addQuestion() {
    const question = this.fb.group({
      text: [],
      keywords: []
    })

    this.questionsForms.push(question);
  }

  deleteQuestion(i) {
    this.questionsForms.removeAt(i);
  }

  addDecision() {
    const decision = this.fb.group({
      meta: [''],
      answer: '',
      resource: ''
    })

    this.decisionsForms.push(decision);
  }

  deleteDecision(i) {
    this.decisionsForms.removeAt(i);
  }




}

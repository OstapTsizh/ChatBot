import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { QuestionsAnswersComponent } from './questions-answers.component';

describe('QuestionsAnswersComponent', () => {
  let component: QuestionsAnswersComponent;
  let fixture: ComponentFixture<QuestionsAnswersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ QuestionsAnswersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(QuestionsAnswersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

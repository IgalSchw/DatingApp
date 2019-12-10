import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http'; // ייבוא ספרייה

@Component({
  selector: 'app-value',
  templateUrl: './value.component.html',
  styleUrls: ['./value.component.css']
})
export class ValueComponent implements OnInit {

  values: any; // הגדרת משתנה

  constructor(private http: HttpClient) { } // בנאי המחלקה עם פרמטר

  ngOnInit() {
    this.getValues(); // קריאה לפונקצייה
  }

  getValues() { // הגדרת פונקצייה לקבלת ערכים
    this.http.get('http://localhost:5000/api/values').subscribe(response => {
      this.values = response;
    }, error => {
      console.log(error);
    });
  }
}

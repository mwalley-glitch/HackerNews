import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  public neweststories: NewestStories[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<NewestStories[]>(baseUrl + 'api/NewestStories/GetNewestStories').subscribe(result => {
      this.neweststories = result;
    }, error => console.error(error));
  }
}

interface NewestStories {
  title: string;
  by: string;
  url: string;
}

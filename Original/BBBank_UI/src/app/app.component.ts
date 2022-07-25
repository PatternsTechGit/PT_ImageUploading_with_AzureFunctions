import { Component, OnInit,ElementRef,ViewChild } from '@angular/core';
import { lineGraphData } from './models/line-graph-data';
import AzureAccessService from './services/azureAccess.service';
import { TransactionService } from './services/transaction.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {

  title = 'BBBankUI';
  lineGraphData: lineGraphData;
  profilePicUrl:string;

  constructor(private transactionService: TransactionService,private azureAccessService: AzureAccessService) {}

  ngOnInit(): void {
    this.transactionService
      .GetLast12MonthBalances('aa45e3c9-261d-41fe-a1b0-5b4dcf79cfd3')
      .subscribe({
        next: (data) => {
          this.lineGraphData = data;
        },
        error: (error) => {
          console.log(error);
        },
      });
  }

  save(files:any) {
    const formData = new FormData();

    if (files[0]) {
      formData.append(files[0].name, files[0]);
    }
    this.azureAccessService
    .uploadImageAndGetUrl(formData)
    .subscribe({
      next: (data) => {
        this.profilePicUrl = data['fullPath'];
      },
      error: (error) => {
        console.log(error);
      },
    });
  }
}

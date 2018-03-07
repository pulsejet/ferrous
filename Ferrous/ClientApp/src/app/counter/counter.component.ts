import { Component } from '@angular/core';
import { DataService } from '../data.service';

@Component({
    selector: 'counter',
    templateUrl: './counter.component.html'
})
export class CounterComponent {
    constructor(private DataServ: DataService) { }

    gotdata: string = "";
    public currentCount = 0;

    public incrementCounter() {
        this.currentCount++;
    }
}

import { Component } from '@angular/core';
import { DataService } from '../data.service';

@Component({
    selector: 'app-counter',
    templateUrl: './counter.component.html'
})
export class CounterComponent {
    constructor(private DataServ: DataService) { }

    gotdata = '';
    public currentCount = 0;

    public incrementCounter() {
        this.currentCount++;
    }
}

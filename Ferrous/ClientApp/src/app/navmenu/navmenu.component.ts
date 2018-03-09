import { Component } from '@angular/core';
import { DataService } from '../data.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-nav-menu',
    templateUrl: './navmenu.component.html',
    styleUrls: ['./navmenu.component.css']
})
export class NavMenuComponent {
    constructor(
        public dataService: DataService,
        public router: Router
    ) {}

    public NavigateLayoutSelect() {
        this.router.navigate(
            ['/location-select', this.dataService.EncodeObject(
                this.dataService.GetLink(this.dataService.GetAPISpec(), 'mark_buildings')), 'marking'
            ]);
    }
}

import { Component } from '@angular/core';
import { API_SPEC } from '../../api.spec';
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
            ['/locationSelect', this.dataService.EncodeObject(
                this.dataService.GetLink(API_SPEC, 'mark_buildings')), 'marking'
            ]);
    }
}

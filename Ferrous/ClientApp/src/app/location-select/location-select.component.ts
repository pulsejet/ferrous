﻿import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Building, Link } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../data.service';

/* Room layout component */
@Component({
    selector: 'app-location-select',
    templateUrl: './location-select.component.html',
})
export class LocationSelectComponent {
    /** Current CLNo */
    public clno: string;
    /** Current contingentArrivalNo */
    public cano: number;
    /** Master Building list */
    public buildings: Building[];

    public urlLink: Link;
    public links: Link[];

    /** constructor for LocationSelectComponent */
    constructor(
        private activatedRoute: ActivatedRoute,
        private titleService: Title,
        private dataService: DataService,
        @Inject('BASE_URL') public baseUrl: string) {

        this.titleService.setTitle('Locations');

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.urlLink = this.dataService.DecodeObject(params['link']);
            this.clno = params['clno'];
        });

        /* Get buildings data */
        this.dataService.GetAllBuildingsExtended(this.urlLink).subscribe(result => {
            this.buildings = result.data;
            this.links = result.links;
        });
    }

    /** Handle table click */
    public handleTableClick(building: Building): void {
        this.dataService.NavigateRoomLayout(this.dataService.GetLinkSelf(building.links), building.location, this.clno);
    }
}

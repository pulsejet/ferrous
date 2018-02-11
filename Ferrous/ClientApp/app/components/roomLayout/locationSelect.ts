﻿import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Building } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

/* Room layout component */
@Component({
    selector: 'locationSelect',
    templateUrl: './locationSelect.html',
    styleUrls: ['../../Custom.css'],
})
export class LocationSelectComponent {
    /** Current CLNo */
    public clno: string;
    /** Current contingentArrivalNo */
    public cano: number;
    /** Master Building list */
    public buildings: Building[];

    /** constructor for LocationSelectComponent */
    constructor(
        private activatedRoute: ActivatedRoute,
        private titleService: Title,
        private dataService: DataService,
        @Inject('BASE_URL') public baseUrl: string) {

        this.titleService.setTitle("Locations");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.clno = params['id'];
            this.cano = params['cano'];
        });

        /* Get buildings data */
        this.dataService.GetAllBuildingsExtended(this.clno).subscribe(result => {
            this.buildings = result;
        });
    }
}
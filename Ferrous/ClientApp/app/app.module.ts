import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { NavMenuComponent } from './navmenu/navmenu.component';
import { HomeComponent } from './home/home.component';

import { ContingentComponent } from './contingent/contingent.component';
import { ContingentDetailsComponent } from './contingent-details/contingent-details.component';
import { ContingentArrivalDialogComponent } from './contingent-arrival-dialog/contingent-arrival-dialog.component'

import { PersonComponent } from './person/person.component';
import { PersonDetailsComponent } from './person-details/person-details.component';

import { RoomLayoutComponent } from './room-layout/room-layout.component';
import { LocationSelectComponent } from './location-select/location-select.component';
import { RoomDialogComponent } from './room-dialog/room-dialog.component'

import { CounterComponent } from './counter/counter.component';
import { MyMaterialClass } from './material-angular.module';
import { DataService } from './data.service';

import { HttpClientModule } from '@angular/common/http';
import { FilterContingents } from './pipes/filter-contingents';
import { ClickStopPropagation } from './helpers';

import 'hammerjs';

const isprod: boolean = !(process.env.ISDEV);

@NgModule({
    bootstrap: [
        AppComponent
    ],
    declarations: [
        AppComponent,
        NavMenuComponent,
        CounterComponent,

        ContingentComponent,
        ContingentDetailsComponent,
        ContingentArrivalDialogComponent,

        PersonComponent,
        PersonDetailsComponent,

        RoomLayoutComponent,
        LocationSelectComponent,
        RoomDialogComponent,

        HomeComponent,

        ClickStopPropagation,
        FilterContingents
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        ServiceWorkerModule.register('/ngsw-worker.js', { enabled: isprod }),

        CommonModule,
        HttpClientModule,
        FormsModule,
        MyMaterialClass,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'counter', component: CounterComponent },

            { path: 'contingent', component: ContingentComponent },
            { path: 'contingentDetails/:id', component: ContingentDetailsComponent },
            { path: 'contingentDetails/:id/:edit', component: ContingentDetailsComponent },

            { path: 'person', component: PersonComponent },
            { path: 'personDetails/:link', component: PersonDetailsComponent },
            { path: 'personDetails/:link/:edit', component: PersonDetailsComponent },

            { path: 'roomLayout/:link/:location/:clno', component: RoomLayoutComponent },
            { path: 'locationSelect/:clno/:cano', component: LocationSelectComponent },

            { path: '**', redirectTo: 'home' }
        ])
    ],
    providers: [
        { provide: 'BASE_URL', useFactory: getBaseUrl },
        DataService
    ],
    entryComponents: [
        ContingentArrivalDialogComponent,
        RoomDialogComponent
    ]
})
export class AppModule { }

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}

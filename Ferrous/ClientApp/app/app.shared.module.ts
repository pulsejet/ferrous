import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';

import { ContingentComponent } from './components/contingent/contingent.component';
import { ContingentDetailsComponent } from './components/contingent-details/contingent-details.component';
import { ContingentArrivalDialogComponent } from './components/contingent-arrival-dialog/contingent-arrival-dialog.component'

import { PersonComponent } from './components/person/person.component';
import { PersonDetailsComponent } from './components/person-details/person-details.component';

import { RoomLayoutComponent } from './components/room-layout/room-layout.component';
import { LocationSelectComponent } from './components/location-select/location-select.component';
import { RoomDialogComponent } from './components/room-dialog/room-dialog.component'

import { CounterComponent } from './components/counter/counter.component';
import { MyMaterialClass } from './components/material-angular.module';
import { DataService } from './data.service';

import { HttpClientModule } from '@angular/common/http';
import { FilterContingents } from './Pipes';
import { ClickStopPropagation } from './components/material-angular.module';

@NgModule({
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
        DataService
    ],
    entryComponents: [
        ContingentArrivalDialogComponent,
        RoomDialogComponent
    ]
})
export class AppModuleShared {
}

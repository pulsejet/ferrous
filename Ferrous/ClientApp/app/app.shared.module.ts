import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';

import { ContingentComponent } from './components/contingent/contingent.component';
import { ContingentDetailsComponent } from './components/contingent/contingentDetails.component';

import { PersonComponent } from './components/person/person';
import { PersonDetailsComponent } from './components/person/personDetails';

import { RoomLayoutComponent } from './components/roomLayout/roomLayout.component';
import { LocationSelectComponent } from './components/roomLayout/locationSelect';

import { CounterComponent } from './components/counter/counter.component';
import { MyMaterialClass } from './components/material-angular';
import { DataService } from './DataService';
import { HttpClientModule } from '@angular/common/http';
import { FilterContingents } from './Pipes';
import { ClickStopPropagation } from './components/material-angular';
import 'hammerjs';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        CounterComponent,

        ContingentComponent,
        ContingentDetailsComponent,

        PersonComponent,
        PersonDetailsComponent,

        RoomLayoutComponent,
        LocationSelectComponent,

        HomeComponent,

        ClickStopPropagation,
        FilterContingents
    ],
    imports: [
        CommonModule,
        HttpModule,
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
            { path: 'personDetails/:id', component: PersonDetailsComponent },
            { path: 'personDetails/:id/:edit', component: PersonDetailsComponent },

            { path: 'roomLayout/:location/:id', component: RoomLayoutComponent },
            { path: 'locationSelect/:id', component: LocationSelectComponent },

            { path: '**', redirectTo: 'home' }
        ])
    ],
    providers: [
        DataService
    ]
})
export class AppModuleShared {
}

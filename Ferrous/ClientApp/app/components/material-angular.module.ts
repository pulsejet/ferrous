//MatAutocompleteModule,
import { MatButtonModule } from '@angular/material/button';
//MatButtonToggleModule,
//MatCardModule,
//MatCheckboxModule,
//MatChipsModule,
//MatDatepickerModule,
import { MatDialogModule } from '@angular/material/dialog';
//MatExpansionModule,
//MatGridListModule,
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input'
import { MatListModule } from '@angular/material/list';
//MatMenuModule,
//MatNativeDateModule,
import { MatPaginatorModule } from '@angular/material/paginator';
//MatProgressBarModule,
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
//MatRadioModule,
//import { MatRippleModule } from '@angular/material/'
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
//MatSliderModule,
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule } from '@angular/material/snack-bar';
//MatSortModule,
//MatTableModule,
//MatTabsModule,
import { MatToolbarModule } from '@angular/material/toolbar';
//MatTooltipModule,
//MatStepperModule

import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';

@NgModule({
    imports: [FlexLayoutModule,
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule, ReactiveFormsModule,
        //MatAutocompleteModule,
        MatButtonModule,
        //MatButtonToggleModule,
        //MatCardModule,
        //MatCheckboxModule,
        //MatChipsModule,
        //MatDatepickerModule,
        MatDialogModule,
        //MatExpansionModule,
        //MatGridListModule,
        MatIconModule,
        MatInputModule,
        MatListModule,
        //MatMenuModule,
        //MatNativeDateModule,
        MatPaginatorModule,
        //MatProgressBarModule,
        MatProgressSpinnerModule,
        //MatRadioModule,
        //MatRippleModule,
        MatSelectModule,
        //MatSliderModule,
        MatSlideToggleModule,
        MatSnackBarModule,
        MatSidenavModule,
        //MatSortModule,
        //MatTableModule,
        //MatTabsModule,
        MatToolbarModule,
        //MatTooltipModule,
        //MatStepperModule,
    ],
    exports: [FlexLayoutModule,
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule, ReactiveFormsModule,
        //MatAutocompleteModule,
        MatButtonModule,
        //MatButtonToggleModule,
        //MatCardModule,
        //MatCheckboxModule,
        //MatChipsModule,
        //MatDatepickerModule,
        MatDialogModule,
        //MatExpansionModule,
        //MatGridListModule,
        MatIconModule,
        MatInputModule,
        MatListModule,
        //MatMenuModule,
        //MatNativeDateModule,
        MatPaginatorModule,
        //MatProgressBarModule,
        MatProgressSpinnerModule,
        //MatRadioModule,
        //MatRippleModule,
        MatSelectModule,
        MatSidenavModule,
        //MatSliderModule,
        MatSlideToggleModule,
        MatSnackBarModule,
        //MatSortModule,
        //MatTableModule,
        //MatTabsModule,
        MatToolbarModule,
        //MatTooltipModule,
        //MatStepperModule,
    ],
})
export class MyMaterialClass { }

import { Directive, HostListener } from "@angular/core";

@Directive({
    selector: "[click-stop-propagation]"
})
export class ClickStopPropagation {
    @HostListener("click", ["$event"])
    public onClick(event: any): void {
        event.stopPropagation();
    }
}
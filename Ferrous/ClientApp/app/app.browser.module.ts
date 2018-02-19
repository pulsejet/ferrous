import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppModuleShared } from './app.shared.module';
import { AppComponent } from './components/app/app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';
import 'hammerjs';

@NgModule({
    bootstrap: [ AppComponent ],
    imports: [
        BrowserModule,
        AppModuleShared,
        BrowserAnimationsModule,
        //ServiceWorkerModule.register('/ngsw-worker.js')
    ],
    providers: [
        { provide: 'BASE_URL', useFactory: getBaseUrl }
    ]
})
export class AppModule {
}

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}

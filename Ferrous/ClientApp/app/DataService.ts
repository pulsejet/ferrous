import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { HttpClient, HttpSentEvent, HttpHeaders } from '@angular/common/http';
import { Contingent, RoomAllocation, Person, Room, Building, ContingentArrival } from './components/interfaces'
import { RequestOptions, Headers, ResponseContentType } from '@angular/http';
import { Router } from '@angular/router';

const API_Contingents_URL: string = '/api/Contingents/';
const API_RoomAllocations_URL: string = '/api/RoomAllocations/';
const API_Buildings_URL: string = '/api/Buildings/';

const API_Rooms_URL: string = '/api/Rooms/';
const API_Rooms_ByLocation_Suffix: string = 'ByLoc/';
const API_Room_Mark_Suffix: string = 'mark/';
const API_Room_Allot_Suffix: string = 'allot/';

const API_People_URL: string = '/api/People/';
const API_ContingentArrivals_URL: string = '/api/ContingentArrivals/';

const SF_RoomLayouts_URL = '/roomTemplates/';

var JSON_HEADERS = new HttpHeaders();
JSON_HEADERS = JSON_HEADERS.set('Content-Type', 'application/json');

/* Main Data Service */
@Injectable()
export class DataService {

    constructor(private http: HttpClient, public router: Router) { }

    /* === Navigate - voids === */

    NavigateContingentsList(): void {
        this.router.navigate(['/contingent/'])
    }

    NavigateContingentDetails(CLNo: string): void {
        this.router.navigate(['/contingentDetails/' + CLNo]);
    }

    NavigatePeopleList(): void {
        this.router.navigate(['/person/']);
    }

    NavigatePersonDetails(MINo: string): void {
        this.router.navigate(['/personDetails/' + MINo]);
    }

    NavigateLayoutSelect(clno:string, contingentArrivalNo: number): void {
        this.router.navigate(['/locationSelect/' + clno + '/' + contingentArrivalNo]);
    }

    /* === Contingents === */

    GetAllContingents(): Observable<Contingent[]> {
        return this.http.get<Contingent[]>(API_Contingents_URL);
    }

    GetContingent(id: string): Observable<Contingent> {
        return this.http.get<Contingent>(API_Contingents_URL + id)
    }

    PutContingent(id: string, body: any): Observable<any> {
        return this.http.put(API_Contingents_URL + id, body, { headers: JSON_HEADERS });
    }

    PostContingent(body: any): Observable<any> {
        return this.http.post(API_Contingents_URL, body, { headers: JSON_HEADERS });
    }

    DeleteContingent(id: string): Observable<any> {
        return this.http.delete(API_Contingents_URL + id);
    }

    /* === People === */
    GetAllPeople(): Observable<Person[]> {
        return this.http.get<Person[]>(API_People_URL);
    }

    GetPerson(id: string): Observable<Person> {
        return this.http.get<Person>(API_People_URL + id)
    }

    PutPerson(id: string, body: any): Observable<any> {
        return this.http.put(API_People_URL + id, body, { headers: JSON_HEADERS });
    }

    PostPerson(body: any): Observable<any> {
        return this.http.post(API_People_URL, body, { headers: JSON_HEADERS });
    }

    DeletePerson(id: string): Observable<any> {
        return this.http.delete(API_People_URL + id);
    }

    /* === Rooms === */

    GetRoomsByLocation(location: string): Observable<Room[]> {
        return this.http.get<Room[]>(API_Rooms_URL + API_Rooms_ByLocation_Suffix +location);
    }

    /* === RoomLayout === */

    GetRoomLayout(location: string): Observable<any> {
        return this.http.get(SF_RoomLayouts_URL + location + '.html', { responseType: 'text' });
    }

    PutRoom(id: string, body: any): Observable<any> {
        return this.http.put(API_Rooms_URL + id, body, { headers: JSON_HEADERS });
    }

    MarkRoom(id: number, status: number): Observable<any> {
        return this.http.get(API_Rooms_URL + API_Room_Mark_Suffix + id + "/" + status, { responseType: 'text' });
    }

    AllotRoom(room: Room, clno: string, cano: number): Observable<RoomAllocation> {
        let url = API_Rooms_URL + API_Room_Allot_Suffix + room.id + '/' + clno + '/' + cano;
        if (room.partialallot || this.RoomCheckPartial(room)) {
            if (room.partialsel == null) throw new Error("Partial number not set!");
            url += '/' + room.partialsel;
        }

        return this.http.get<RoomAllocation>(url);
    }

    RoomCheckOccupied(room: Room): boolean {
        return this.RoomGetPartialNo(room) < 0;
    }

    RoomCheckPartial(room: Room): boolean {
        return this.RoomGetPartialNo(room) > 0;
    }

    RoomGetPartialNo(room: Room): number {
        let count: number = 0;
        for (let roomA of room.roomAllocation) {
            if (roomA.partial != null) count += Number(roomA.partial);
        }
        return count;
    }

    /* === RoomAllocations === */

    UnllocateRoom(sno: number): Observable<any> {
        return this.http.delete(API_RoomAllocations_URL + sno)
    }

    /* === Buildings === */

    GetAllBuildings(): Observable<Building[]> {
        return this.http.get<Building[]>(API_Buildings_URL)
    }

    GetAllBuildingsExtended(clno: string): Observable<Building[]> {
        return this.http.get<Building[]>(API_Buildings_URL + "e/" + clno)
    }

    GetBuilding(loc: string): Observable<Building> {
        return this.http.get<Building>(API_Buildings_URL + loc)
    }

    /* === ContingentArrival === */

    PostContingentArrival(body: any): Observable<ContingentArrival>  {
        return this.http.post<ContingentArrival>(API_ContingentArrivals_URL, body, { headers: JSON_HEADERS });
    }

    DeleteContingentArrival(id: number): Observable<any> {
        return this.http.delete(API_ContingentArrivals_URL + id);
    }

    /* === Quick Extras which shouldn't be here === */

    CheckValidNumber(num: number, min: number = -999999, max: number = 999999): boolean {
        return (num != null &&
            !isNaN(Number(num)) &&
            Number(num) >= min &&
            Number(num) <= max)
    }
}
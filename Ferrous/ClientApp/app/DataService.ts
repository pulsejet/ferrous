import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Contingent, RoomAllocation, Person, Room, Building, ContingentArrival, EnumContainer, Link } from './components/interfaces'
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

const API_Logout_URL: string = '/api/login/logout';

var JSON_HEADERS = new HttpHeaders();
JSON_HEADERS = JSON_HEADERS.set('Content-Type', 'application/json');

/* Main Data Service */
@Injectable()
export class DataService {

    public passedData: any;
    public passedLinks: Link[];

    constructor(private http: HttpClient, public router: Router) { }

    SetPassedData(data: any): void {
        this.passedData = data;
    }

    GetPassedData(): any {
        return this.passedData;
    }

    SetPassedLinks(links: Link[]): void {
        this.passedLinks = links;
    }

    PopPassedLinks(): any {
        let ans = this.passedLinks;
        this.passedLinks = [];
        return ans;
    }

    GetLink(links: Link[], rel: string = "self", encoded: boolean = false): any {
        let found = links.find(x => x.rel === rel);
        if (found == null) return null;
        if (encoded) return this.EncodeObject(found);
        return found;
    }

    GetLinkSelf(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "self", encoded); }
    GetLinkUpdate(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "update", encoded); }
    GetLinkDelete(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "delete", encoded); }
    GetLinkCreate(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "create", encoded); }

    FireLinkUpdate(links: Link[], body: any): Observable<any> { return this.FireLink(this.GetLinkUpdate(links), body); }
    FireLinkDelete(links: Link[]): Observable<any> { return this.FireLink(this.GetLinkDelete(links)); }

    EncodeObject(o: any): string { return btoa(JSON.stringify(o)) }
    DecodeObject(s: string): any { return JSON.stringify(atob(s)) }

    FireLink(link: Link, body: any = null): Observable<any> {
        switch (link.method) {
            case "GET":
                return this.http.get(link.href);
            case "POST":
                return this.http.post(link.href, body, { headers: JSON_HEADERS });
            case "PUT":
                return this.http.put(link.href, body, { headers: JSON_HEADERS });                
            case "DELETE":
                return this.http.delete(link.href);
            default:
                throw new Error("no method defined for " + link.href);
        }
    }

    /* === Navigate - voids === */

    /**
     * Navigate to list of Contingents
     */
    NavigateContingentsList(): void {
        this.router.navigate(['/contingent/']);
    }

    /**
     * Navigate to ContingentDetails
     * @param CLNo CLNo of particular contingent
     */
    NavigateContingentDetails(link: Link, newRecord: boolean = false): void {
        this.router.navigate(['/contingentDetails/' + this.EncodeObject(link) + (newRecord ? '/1' : '')]);
    }

    /**
     * Navigate to list of People
     */
    NavigatePeopleList(): void {
        this.router.navigate(['/person/']);
    }

    /**
     * Navigate to PersonDetails
     * @param MINo MINo of particular person
     */
    NavigatePersonDetails(MINo: string): void {
        this.router.navigate(['/personDetails/' + MINo]);
    }

    /**
     * Navigate to Location Selection 
     * @param clno CLNo of the contingent being allocated rooms; Use "mark" if marking
     * @param contingentArrivalNo contingentArrivalNo for which rooms are being allocated
     */
    NavigateLayoutSelect(clno:string, contingentArrivalNo: number): void {
        this.router.navigate(['/locationSelect/' + clno + '/' + contingentArrivalNo]);
    }

    /* === Contingents === */

    /**
     * All Contingent[]
     */
    GetAllContingents(): Observable<EnumContainer> {
        return this.http.get<EnumContainer>(API_Contingents_URL);
    }
    
    /* === People === */

    /**
     * All People
     */
    GetAllPeople(): Observable<Person[]> {
        return this.http.get<Person[]>(API_People_URL);
    }

    /**
     * Person Details
     * @param id MINo of Person
     */
    GetPerson(id: string): Observable<Person> {
        return this.http.get<Person>(API_People_URL + id);
    }

    /**
     * PUT update to existing Person
     * @param id MINo of Person
     * @param body JSON of Person
     */
    PutPerson(id: string, body: any): Observable<any> {
        return this.http.put(API_People_URL + id, body, { headers: JSON_HEADERS });
    }

    /**
     * POST new Person
     * @param body JSON of Person
     */
    PostPerson(body: any): Observable<any> {
        return this.http.post(API_People_URL, body, { headers: JSON_HEADERS });
    }

    /**
     * DELETE Person without prompting
     * @param id
     */
    DeletePerson(id: string): Observable<any> {
        return this.http.delete(API_People_URL + id);
    }

    /* === Rooms === */

    /**
     * (Deprecated) Get Room[] from a given location
     * @param location Location code
     */
    GetRoomsByLocation(location: string): Observable<Room[]> {
        return this.http.get<Room[]>(API_Rooms_URL + API_Rooms_ByLocation_Suffix +location);
    }

    /* === RoomLayout === */

    /**
     * Get room layout HTML
     * @param location Location code
     */
    GetRoomLayout(location: string): Observable<any> {
        return this.http.get(SF_RoomLayouts_URL + location + '.html', { responseType: 'text' });
    }

    /**
     * PUT update to existing Room
     * @param id RoomId of Room
     * @param body JSON of Room
     */
    PutRoom(id: string, body: any): Observable<any> {
        return this.http.put(API_Rooms_URL + id, body, { headers: JSON_HEADERS });
    }

    /**
     * Mark room with a status
     * @param id RoomId of Room
     * @param status Status to be marked
     */
    MarkRoom(id: number, status: number): Observable<any> {
        return this.http.get(API_Rooms_URL + API_Room_Mark_Suffix + id + "/" + status, { responseType: 'text' });
    }

    /**
     * Try to allot room to contingent
     * @param room Room object to be allocated
     * @param clno CLNo of contingent for allocation
     * @param cano contingentArrivalNo to which room is to be allocated
     */
    AllotRoom(room: Room, clno: string, cano: number): Observable<RoomAllocation> {
        let url = API_Rooms_URL + API_Room_Allot_Suffix + room.roomId + '/' + clno + '/' + cano;
        if (room.partialallot || this.RoomCheckPartial(room)) {
            if (room.partialsel == null) throw new Error("Partial number not set!");
            url += '/' + room.partialsel;
        }

        return this.http.get<RoomAllocation>(url);
    }

    /**
     * Returns true if room is fully occupied
     * @param room Room to check
     */
    RoomCheckOccupied(room: Room): boolean {
        return this.RoomGetPartialNo(room) < 0;
    }

    /**
     * Returns if room is partially occupied (returns true even if partial is more than capacity)
     * @param room Room to check
     */
    RoomCheckPartial(room: Room): boolean {
        return this.RoomGetPartialNo(room) > 0;
    }

    /**
     * Get number of people occupying room partially
     * @param room Room to check
     */
    RoomGetPartialNo(room: Room): number {
        let count: number = 0;
        for (let roomA of room.roomAllocation) {
            if (roomA.partial != null) count += Number(roomA.partial);
        }
        return count;
    }

    /* === RoomAllocations === */

    /**
     * DELETE a RoomAllocation
     * @param sno SNo of RoomAllocation
     */
    UnllocateRoom(sno: number): Observable<any> {
        return this.http.delete(API_RoomAllocations_URL + sno);
    }

    /* === Buildings === */

    /**
     * All Building[]
     */
    GetAllBuildings(): Observable<Building[]> {
        return this.http.get<Building[]>(API_Buildings_URL);
    }

    /**
     * Gets contingent-specific extended Building[]
     * @param clno CLNo of Contingent to lookup
     */
    GetAllBuildingsExtended(clno: string): Observable<Building[]> {
        return this.http.get<Building[]>(API_Buildings_URL + "e/" + clno);
    }

    /**
     * Gets a Building object with Room and RoomAllocation objects
     * @param loc Location code of Building
     */
    GetBuilding(loc: string): Observable<Building> {
        return this.http.get<Building>(API_Buildings_URL + loc);
    }

    /* === ContingentArrival === */

    /**
     * POST a new ContingentArrival
     * @param body
     */
    PostContingentArrival(body: any): Observable<ContingentArrival>  {
        return this.http.post<ContingentArrival>(API_ContingentArrivals_URL, body, { headers: JSON_HEADERS });
    }

    /**
     * DELETE an empty ContingentArrival
     * @param id contingentArrivalNo
     */
    DeleteContingentArrival(id: number): Observable<any> {
        return this.http.delete(API_ContingentArrivals_URL + id);
    }

    /* === Quick Extras which shouldn't be here === */

    /**
     * Returns true if object is a valid number (in range)
     * @param num object to be checked
     * @param min optional minimum bound (defaults to -999999)
     * @param max optional maximum bound (defaults to +999999)
     */
    CheckValidNumber(num: number, min: number = -999999, max: number = 999999): boolean {
        return (num != null &&
            !isNaN(Number(num)) &&
            Number(num) >= min &&
            Number(num) <= max);
    }

    /**
     * End the session
     */
    Logout(): Observable<any> {
        return this.http.get(API_Logout_URL);
    }
}
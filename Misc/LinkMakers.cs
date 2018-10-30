using Ferrous.Controllers;
using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ferrous.Misc
{
    public class LinksMaker
    {
        readonly private ClaimsPrincipal User;
        readonly private IUrlHelper Url;

        public LinksMaker(ClaimsPrincipal User, IUrlHelper Url)
        {
            this.User = User;
            this.Url = Url;
        }

        /// <summary>
        /// Generate the API Spec
        /// </summary>
        /// <returns>List of links defined in API_SPEC</returns>
        public List<Link> API_SPEC() =>
            new LinkHelper()
                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingents), null, "contingents")
                .AddLink(nameof(ContingentsController.FindContingent), null, "find_contingent", true)

                .SetOptions(User, typeof(BuildingsController), Url)
                .AddLink(nameof(BuildingsController.GetBuildingsExtended), new { id = "mark", cano = "mark" }, "mark_buildings")
                .AddLink(nameof(BuildingsController.GetStatsUpdate), null, "stats-update")

                .SetOptions(User, typeof(PeopleController), Url)
                .AddLink(nameof(PeopleController.GetPeople), null, "people")
                .AddLink(nameof(PeopleController.GetPersonForward), null, "person_forward", true)
                .AddLink(nameof(PeopleController.UploadSheet), null, "upload-people-sheet")
                .AddLink(nameof(PeopleController.FindPerson), null, "find-person", true)

                .SetOptions(User, typeof(LoginController), Url)
                .AddLink(nameof(LoginController.login), null, "login", true)
                .AddLink(nameof(LoginController.Logout), null, "logout")
                .AddLink(nameof(LoginController.GetUser), null, "getuser")

                .SetOptions(User, typeof(ContingentArrivalsController), Url)
                .AddLink(nameof(ContingentArrivalsController.GetDesk1), null, "desk1", true)
                .AddLink(nameof(ContingentArrivalsController.GetStats), null, "ca-stats")

                .SetOptions(User, typeof(ExternalController), Url)
                .AddLink(nameof(ExternalController.PostForm1), null, "postform1")
                .AddLink(nameof(ExternalController.ValidatePostForm1), null, "validatepostform1")

                .SetOptions(User, typeof(RoomsController), Url)
                .AddLink(nameof(RoomsController.UploadSheet), null, "upload-sheet")
                .AddLink(nameof(RoomsController.UploadSheetSample), null, "upload-sheet-sample")

                /* Add websocket */
                .AddAbsoluteContentLink(WebSocketHubs.BuildingUpdateHub.BuildingWebsocketUrl, "building_websocket")
                .AddStringLink(nameof(WebSocketHubs.BuildingUpdateHub.JoinBuilding), "building_websocket_join")

                .GetLinks();

        /// <summary>
        /// Fill Links object for Contingents.
        /// </summary>
        /// <param name="contingent">Contingent object</param>
        /// <returns></returns>
        public void FillContingentsLinks(Contingent contingent)
        {
            var idObject = new { id = contingent.ContingentLeaderNo };

            contingent.Links = new LinkHelper()
                /* Contingent Actions */
                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingent), idObject)
                .AddLink(nameof(ContingentsController.PutContingent), idObject)
                .AddLink(nameof(ContingentsController.DeleteContingent), idObject)

                /* POST a ContingentArrival */
                .SetOptions(User, typeof(ContingentArrivalsController), Url)
                .AddLink(nameof(ContingentArrivalsController.PostContingentArrival), null, "create_contingent_arrival")
                .GetLinks();

            foreach (var ca in contingent.ContingentArrival) {
                FillContingentArrivalLinks(ca);
            }

            foreach (var ra in contingent.RoomAllocation) {
                FillRoomAllocationLinks(ra);
            }

            foreach (var person in contingent.Person) {
                FillPersonLinks(person);
            }
        }

        /// <summary>
        /// Fills Links object for Building.
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        public void FillBuildingsLinks(Building building, string clno, int cano)
        {
            var idObject = new { id = building.Location, clno, cano };

            var linkHelper = new LinkHelper()
                   .SetOptions(User, typeof(BuildingsController), Url)
                   .AddLink(nameof(BuildingsController.GetBuilding), idObject)
                   .AddLink(nameof(BuildingsController.PutBuilding), idObject)
                   .AddLink(nameof(BuildingsController.DeleteBuilding), idObject)

                   .SetOptions(User, typeof(RoomsController), Url)
                   .AddLink(nameof(RoomsController.GetRoomList), new {clno, cano}, "list_rooms")
                   .AddLink(nameof(RoomsController.MarkRooms), null, "mark", true);

            if (clno != "mark") {
                linkHelper.SetOptions(User, typeof(ExportController), Url)
                          .AddLink(nameof(ExportController.GetContingentArrivalBill), new {id = cano}, "bill")
                          .SetOptions(User, typeof(RoomsController), Url)
                          .AddLink(nameof(RoomsController.AllotRooms), new {clno, cano}, "allot")
                          .SetOptions(User, typeof(ContingentArrivalsController), Url)
                          .AddLink(nameof(ContingentArrivalsController.GetContingentArrival), new {id = cano}, "get-ca");
            }

            building.Links = linkHelper.GetLinks();

            if (building.Room != null) {
                foreach (var room in building.Room) {
                    FillRoomLinks(room, clno, cano);
                }
            }
        }

        /// <summary>
        /// Fill Links object for ContingentArrival
        /// </summary>
        /// <param name="contingentArrival">ContingentArrival object</param>
        /// <returns></returns>
        public void FillContingentArrivalLinks(ContingentArrival contingentArrival)
        {
            var idObject = new { id = contingentArrival.ContingentArrivalNo };
            var idObjectBuilding = new { id = contingentArrival.ContingentLeaderNo, cano = contingentArrival.ContingentArrivalNo };
            var desk1obj = new { cano = contingentArrival.ContingentArrivalNo};
            contingentArrival.Links = new LinkHelper()
                .SetOptions(User, typeof(ContingentArrivalsController), Url)
                .AddLink(nameof(ContingentArrivalsController.PutContingentArrival), idObject)
                .AddLink(nameof(ContingentArrivalsController.DeleteContingentArrival), idObject)
                .AddLink(nameof(ContingentArrivalsController.PostCAPerson), desk1obj, "add_caperson")
                .AddLink(nameof(ContingentArrivalsController.ApproveContingentArrival), desk1obj, "approve")
                .AddLink(nameof(ContingentArrivalsController.UnApproveContingentArrival), desk1obj, "unapprove")

                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingent), new { id = contingentArrival.ContingentLeaderNo }, "contingent")

                .SetOptions(User, typeof(ExportController), Url)
                .AddLink(nameof(ExportController.GetContingentArrivalBill), idObject, "bill")

                .SetOptions(User, typeof(RoomAllocationsController), Url)
                .AddLink(nameof(RoomAllocationsController.PostRoomAllocation), null, "create_room_allocation")

                .SetOptions(User, typeof(BuildingsController), Url)
                .AddLink(nameof(BuildingsController.GetBuildingsExtended), idObjectBuilding, "buildings")
                .AddLink(nameof(BuildingsController.GetBuildingsMin), idObjectBuilding, "buildings-min")

                .GetLinks();
        }

        public void FillCAPersonLinks(CAPerson caPerson) {
            caPerson.Links = new LinkHelper()
                .SetOptions(User, typeof(ContingentArrivalsController), Url)
                .AddLink(nameof(ContingentArrivalsController.DeleteCAPerson), new { id = caPerson.Sno })
                .GetLinks();
        }

        /// <summary>
        /// Fill Links object for Room Allocation
        /// </summary>
        /// <param name="roomAllocation">RoomAllocation object</param>
        public void FillRoomAllocationLinks(RoomAllocation roomAllocation)
        {
            var idObject = new { id = roomAllocation.Sno };
            roomAllocation.Links = new LinkHelper()
                .SetOptions(User, typeof(RoomAllocationsController), Url)
                .AddLink(nameof(RoomAllocationsController.DeleteRoomAllocation), idObject)
                .GetLinks();
        }

        /// <summary>
        /// Fill Links object for Room
        /// </summary>
        /// <param name="room">Room object</param>
        public void FillRoomLinks(Room room, string clno, int cano)
        {
            var idObject = new { id = room.RoomId };

            var linkHelper = new LinkHelper()
                .SetOptions(User, typeof(RoomsController), Url)
                .AddLink(nameof(RoomsController.GetRoom), new { id = room.RoomId, clno, cano })
                .AddLink(nameof(RoomsController.PutRoom), idObject)
                .AddLink(nameof(RoomsController.DeleteRoom), idObject);

            room.Links = linkHelper.GetLinks();

            foreach (var roomA in room.RoomAllocation) {
                FillRoomAllocationLinks(roomA);
            }
        }

        /// <summary>
        /// Fills links object for Person object
        /// </summary>
        /// <param name="person">Person object to fill</param>
        public void FillPersonLinks(Person person)
        {
            var idObject = new { id = person.Mino };
            person.links = new LinkHelper()
                .SetOptions(User, typeof(PeopleController), Url)
                .AddLink(nameof(PeopleController.GetPerson), idObject)
                .AddLink(nameof(PeopleController.PutPerson), idObject)
                .AddLink(nameof(PeopleController.DeletePerson), idObject)

                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingent), new { id = person.ContingentLeaderNo }, "contingent")

                .GetLinks();
        }

    }
}

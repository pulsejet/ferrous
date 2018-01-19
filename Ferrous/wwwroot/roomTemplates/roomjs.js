var selectedRooms = [];
$( document ).ready(function() {
    $('.room').each(function() {
        $(this).html( $(this).attr('capacity') );
    });

    $(".room").click(function(){
        var current_class = $(this).attr("class");
        if (current_class.indexOf("sel") >=0) 
        {
            $(this).attr("class", $(this).attr("oldclass"));
            selectedRooms.splice(selectedRooms.indexOf($(this).attr('roomid')), 1);
            return;
        }
        
        if (current_class.indexOf("empty") >=0 || ($("#marking").is(':checked')))
        {
            $(this).attr("oldclass", $(this).attr("class"));
            $(this).attr("class", "room sel");
            selectedRooms.push($(this).attr('roomid'));
            if ($("#marking").is(':checked')) return;
        }
    });
});

function allot() {
    runop('allot', '');
}

function markunavailable() {
    runop('markstatus', '&status=0');
}

function markempty() {
    runop('markstatus', '&status=1');
}

function runop(op,extra) {
    selectedRooms.forEach(function (entry) {
        $.ajax({
            url: '/room/' + op +'?id=' + entry + extra,
            success: function (data) {
                console.log(data + " done");
            },
            error: function () {
                alert(entry + " failed");
            }
        });
    });
    
    $(document).ajaxStop(function () { window.location.reload(); });
}
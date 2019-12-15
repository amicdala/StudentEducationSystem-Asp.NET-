
var second = 0, minute = 60, hour = 0;

function show() {
    if (second != 0) second = second - 1;
    else {
        second = 59;
        if (minute != 0) minute = minute - 1;
        else {
            //SÜRE DOLDU    
        }

    }
    $("#counter").html(hour + " : " + minute + " : " + second);
}
$(document).ready(function () {
    $("#counter").html("0 : 60 : 0");
    setInterval(show, 1000);
});

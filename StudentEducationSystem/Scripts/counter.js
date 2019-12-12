$(document).ready(function () {
    var saniye = 3600;
    var sayacYeri = $("count-numbers");

    $.sayimiBaslat = function () {
        if (saniye > 1) {
            saniye--;
            sayacYeri.text(saniye);
        } else {
            $("count-numbers").text("Test Bitti");
        }
    }

    sayacYeri.text(saniye);
    setInterval("$.sayimiBaslat()", 1000);
})
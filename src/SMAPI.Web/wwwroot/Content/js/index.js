$(document).ready(function () {
    var pufferchick = $("#pufferchick");
    $(".download").each(function (index, element) {
        $(element).hover(function () {
            pufferchick.attr("src", "Content/images/pufferchick-cool.png");
        }, function () {
            pufferchick.attr("src", "favicon.ico");
        })
    });
});

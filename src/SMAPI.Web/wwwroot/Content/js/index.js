document.addEventListener('DOMContentLoaded', setupHover, false);

function setupHover() {
    var pufferchick = document.getElementById("pufferchick");
    var downloadLinks = document.getElementsByClassName("download");

    for (var downloadLink of downloadLinks) {
        downloadLink.addEventListener("mouseenter", function () {
            pufferchick.src = "Content/images/pufferchick-cool.png";
        });

        downloadLink.addEventListener("mouseleave", function () {
            pufferchick.src = "favicon.ico";
        });
    }
}

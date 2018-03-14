/* globals $ */

var smapi = smapi || {};
var app;
smapi.logParser = function (data, sectionUrl) {
    // internal filter counts
    var stats = data.stats = {
        modsShown: 0,
        modsHidden: 0
    };
    function updateModFilters() {
        // counts
        stats.modsShown = 0;
        stats.modsHidden = 0;
        for (var key in data.showMods) {
            if (data.showMods.hasOwnProperty(key)) {
                if (data.showMods[key])
                    stats.modsShown++;
                else
                    stats.modsHidden++;
            }
        }
    }

    // set local time started
    if(data)
        data.localTimeStarted = ("0" + data.logStarted.getHours()).slice(-2) + ":" + ("0" + data.logStarted.getMinutes()).slice(-2);

    // init app
    app = new Vue({
        el: '#output',
        data: data,
        computed: {
            anyModsHidden: function () {
                return stats.modsHidden > 0;
            },
            anyModsShown: function () {
                return stats.modsShown > 0;
            }
        },
        methods: {
            toggleLevel: function(id) {
                this.showLevels[id] = !this.showLevels[id];
            },

            toggleMod: function (id) {
                var curShown = this.showMods[id];

                // first filter: only show this by default
                if (stats.modsHidden === 0) {
                    this.hideAllMods();
                    this.showMods[id] = true;
                }

                // unchecked last filter: reset
                else if (stats.modsShown === 1 && curShown)
                    this.showAllMods();

                // else toggle
                else
                    this.showMods[id] = !this.showMods[id];

                updateModFilters();
            },

            showAllMods: function () {
                for (var key in this.showMods) {
                    if (this.showMods.hasOwnProperty(key)) {
                        this.showMods[key] = true;
                    }
                }
                updateModFilters();
            },

            hideAllMods: function () {
                for (var key in this.showMods) {
                    if (this.showMods.hasOwnProperty(key)) {
                        this.showMods[key] = false;
                    }
                }
                updateModFilters();
            },

            filtersAllow: function(modId, level) {
                return this.showMods[modId] !== false && this.showLevels[level] !== false;
            }
        }
    });

    /**********
    ** Upload form
    *********/
    var error = $("#error");
    
    $("#upload-button").on("click", function() {
        $("#input").val("");
        $("#popup-upload").fadeIn();
    });

    var closeUploadPopUp = function() {
        $("#popup-upload").fadeOut(400);
    };

    $("#popup-upload").on({
        'dragover dragenter': function(e) {
            e.preventDefault();
            e.stopPropagation();
        },
        'drop': function(e) {
            $("#uploader").attr("data-text", "Reading...");
            $("#uploader").show();
            var dataTransfer = e.originalEvent.dataTransfer;
            if (dataTransfer && dataTransfer.files.length) {
                e.preventDefault();
                e.stopPropagation();
                var file = dataTransfer.files[0];
                var reader = new FileReader();
                reader.onload = $.proxy(function(file, $input, event) {
                    $input.val(event.target.result);
                    $("#uploader").fadeOut();
                    $("#submit").click();
                }, this, file, $("#input"));
                reader.readAsText(file);
            }
        },
        'click': function(e) {
            if (e.target.id === "popup-upload")
                closeUploadPopUp();
        }
    });

    $("#submit").on("click", function() {
        $("#popup-upload").fadeOut();
        var paste = $("#input").val();
        if (paste) {
            //memory = "";
            $("#uploader").attr("data-text", "Saving...");
            $("#uploader").fadeIn();
            $
                .ajax({
                    type: "POST",
                    url: sectionUrl + "/save",
                    data: JSON.stringify(paste),
                    contentType: "application/json" // sent to API
                })
                .fail(function(xhr, textStatus) {
                    $("#uploader").fadeOut();
                    error.html('<h1>Parsing failed!</h1>Parsing of the log failed, details follow.<br />&nbsp;<p>Stage: Upload</p>Error: ' + textStatus + ': ' + xhr.responseText + "<hr /><pre>" + $("#input").val() + "</pre>");
                })
                .then(function(data) {
                    $("#uploader").fadeOut();
                    if (!data.success)
                        error.html('<h1>Parsing failed!</h1>Parsing of the log failed, details follow.<br />&nbsp;<p>Stage: Upload</p>Error: ' + data.error + "<hr /><pre>" + $("#input").val() + "</pre>");
                    else
                        location.href = (sectionUrl.replace(/\/$/, "") + "/" + data.id);
                });
        } else {
            alert("Unable to parse log, the input is empty!");
            $("#uploader").fadeOut();
        }
    });

    $(document).on("keydown", function(e) {
        if (e.which === 27) {
            if ($("#popup-upload").css("display") !== "none" && $("#popup-upload").css("opacity") === 1) {
                closeUploadPopUp();
            }
        }
    });
    $("#cancel").on("click", closeUploadPopUp);

    if (data.showPopup)
        $("#popup-upload").fadeIn();

};

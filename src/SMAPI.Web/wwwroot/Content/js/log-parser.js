/* globals $, LZString */

$(function() {
    /*********
    ** Initialisation
    *********/
    var stage,
        flags = $("#modflags"),
        output = $("#output"),
        filters = 0,
        memory = "",
        versionInfo,
        modInfo,
        modMap,
        modErrors,
        logInfo,
        templateBody = $("#template-body").text(),
        templateModentry = $("#template-modentry").text(),
        templateCss = $("#template-css").text(),
        templateLogentry = $("#template-logentry").text(),
        templateLognotice = $("#template-lognotice").text(),
        regexInfo = /\[[\d\:]+ INFO  SMAPI] SMAPI (.*?) with Stardew Valley (.*?) on (.*?)\n/g,
        regexMods = /\[[^\]]+\] Loaded \d+ mods:(?:\n\[[^\]]+\]    .+)+/g,
        regexLog = /\[([\d\:]+) (TRACE|DEBUG|INFO|WARN|ALERT|ERROR) ? ([^\]]+)\] ?((?:\n|.)*?)(?=(?:\[\d\d:|$))/g,
        regexMod = /\[(?:.*?)\] *(.*?) (\d+\.?(?:.*?))(?: by (.*?))? \|(?:.*?)$/gm,
        regexDate = /\[\d{2}:\d{2}:\d{2} TRACE SMAPI\] Log started at (.*?) UTC/g,
        regexPath = /\[\d{2}:\d{2}:\d{2} DEBUG SMAPI\] Mods go here: (.*?)(?:\n|$)/g;

    $("#tabs li:not(.notice)").on("click", function(evt) {
        var t = $(evt.currentTarget);
        t.toggleClass("active");
        $("#output").toggleClass(t.text().toLowerCase());
    });
    $("#upload-button").on("click", function() {
        memory = $("#input").val() || "";
        $("#input").val("");
        $("#popup-upload").fadeIn();
    });
    var proxies = [
        "https://cors-anywhere.herokuapp.com/",
        "https://galvanize-cors-proxy.herokuapp.com/"
    ];
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
        }
    });

    $("#submit").on("click", function() {
        $("#popup-upload").fadeOut();
        if ($("#input").val()) {
            memory = "";
            var raw = $("#input").val();
            var paste = LZString.compressToUTF16(raw);
            logSize("Raw", raw);
            logSize("Compressed", paste);
            if (paste.length * 2 > 524288) {
                $("#output").html('<div id="log" class="color-red"><h1>Unable to save!</h1>This log cannot be saved due to its size.<hr />' + $("#input").val() + "</div>");
                return;
            }
            console.log("paste:", paste);
            var packet = {
                api_dev_key: "b8219d942109d1e60ebb14fbb45f06f9",
                api_option: "paste",
                api_paste_private: 1,
                api_paste_code: paste,
                api_paste_expire_date: "1W"
            };
            $("#uploader").attr("data-text", "Saving...");
            $("#uploader").fadeIn();
            var uri = proxies[Math.floor(Math.random() * proxies.length)] + "pastebin.com/api/api_post.php";
            console.log(packet, uri);
            $.post(uri, packet, function(data) {
                $("#uploader").fadeOut();
                console.log("Result: ", data);
                if (data.substring(0, 15) === "Bad API request")
                    $("#output").html('<div id="log" class="color-red"><h1>Parsing failed!</h1>Parsing of the log failed, details follow.<br />&nbsp;<p>Stage: Upload</p>Error: ' + data + "<hr />" + $("#input").val() + "</div>");
                else if (data)
                    location.href = "?" + data.split("/").pop();
                else
                    $("#output").html('<div id="log" class="color-red"><h1>Parsing failed!</h1>Parsing of the log failed, details follow.<br />&nbsp;<p>Stage: Upload</p>Error: Received null response<hr />' + $("#input").val() + "</div>");
            });
        } else {
            alert("Unable to parse log, the input is empty!");
            $("#uploader").fadeOut();
        }
    });
    $("#cancel").on("click", function() {
        $("#popup-upload").fadeOut(400, function() {
            $("#input").val(memory);
            memory = "";
        });
    });
    $("#closeraw").on("click", function() {
        $("#popup-raw").fadeOut(400);
    });
    if (location.search) {
        getData();
    }
    else
        $("#popup-upload").fadeIn();


    /*********
    ** Helpers
    *********/
    function logSize(id, str) {
        console.log(id + ":", str.length * 2, "bytes", Math.round(str.length / 5.12) / 100, "kb");
    }

    function modClicked(evt) {
        var id = $(evt.currentTarget).attr("id").split("-")[1],
            cls = "mod-" + id;
        if (output.hasClass(cls))
            filters--;
        else
            filters++;
        output.toggleClass(cls);
        if (filters === 0) {
            output.removeClass("modfilter");
        } else {
            output.addClass("modfilter");
        }
    }

    function removeFilter() {
        for (var c = 0; c < modInfo.length; c++) {
            output.removeClass("mod-" + c);
        }
        filters = 0;
        output.removeClass("modfilter");
    }

    function selectAll() {
        for (var c = 0; c < modInfo.length; c++) {
            output.addClass("mod-" + c);
        }
        filters = modInfo.length;
        output.addClass("modfilter");
    }

    function parseData() {
        stage = "parseData.pre";
        var data = $("#input").val();
        if (!data) {
            stage = "parseData.checkNullData";
            throw new Error("Field `data` is null");

        }
        var dataInfo = regexInfo.exec(data) || regexInfo.exec(data) || regexInfo.exec(data),
            dataMods = regexMods.exec(data) || regexMods.exec(data) || regexMods.exec(data),
            dataDate = regexDate.exec(data) || regexDate.exec(data) || regexDate.exec(data),
            dataPath = regexPath.exec(data) || regexPath.exec(data) || regexPath.exec(data),
            match;
        console.log("dataInfo:", dataInfo);
        console.log("dataMods:", dataMods);
        console.log("dataDate:", dataDate);
        console.log("dataPath:", dataPath);
        stage = "parseData.doNullCheck";
        if (!dataInfo)
            throw new Error("Field `dataInfo` is null");
        if (!dataMods)
            throw new Error("Field `dataMods` is null");
        if (!dataPath)
            throw new Error("Field `dataPath` is null");
        dataMods = dataMods[0];
        stage = "parseData.setupDefaults";
        modMap = {
            "SMAPI": 0
        };
        modErrors = {
            "SMAPI": 0,
            "Console.Out": 0
        };
        logInfo = [];
        modInfo = [
            ["SMAPI", dataInfo[1], "Zoryn, CLxS & Pathoschild"]
        ];
        stage = "parseData.parseInfo";
        var date = dataDate ? new Date(dataDate[1] + "Z") : null;
        versionInfo = [dataInfo[1], dataInfo[2], dataInfo[3], date ? date.getFullYear() + "-" + ("0" + date.getMonth().toString()).substr(-2) + "-" + ("0" + date.getDay().toString()).substr(-2) + " at " + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds() + " " + date.toLocaleTimeString("en-us", { timeZoneName: "short" }).split(" ")[2] : "No timestamp found", dataPath[1]];
        stage = "parseData.parseMods";
        while ((match = regexMod.exec(dataMods))) {
            modErrors[match[1]] = 0;
            modMap[match[1]] = modInfo.length;
            modInfo.push([match[1], match[2], match[3] ? ("by " + match[3]) : "Unknown author"]);
        }
        stage = "parseData.parseLog";
        while ((match = regexLog.exec(data))) {
            if (match[2] === "ERROR")
                modErrors[match[3]]++;
            logInfo.push([match[1], match[2], match[3], match[4]]);
        }
        stage = "parseData.post";
        modMap["Console.Out"] = modInfo.length;
        modInfo.push(["Console.Out", "", ""]);
    }

    function renderData() {
        stage = "renderData.pre";
        output.html(prepare(templateBody, versionInfo));
        var modslist = $("#modslist"), log = $("#log"), modCache = [], y = 0;
        for (; y < modInfo.length; y++) {
            var errors = modErrors[modInfo[y][0]],
                err, cls = "color-red";
            if (errors === 0) {
                err = "No Errors";
                cls = "color-green";
            } else if (errors === 1)
                err = "1 Error";
            else
                err = errors + " Errors";
            modCache.push(prepare(templateModentry, [y, modInfo[y][0], modInfo[y][1], modInfo[y][2], cls, err]));
        }
        modslist.append(modCache.join(""));
        for (var z = 0; z < modInfo.length; z++)
            $("#modlink-" + z).on("click", modClicked);
        var flagCache = [];
        for (var c = 0; c < modInfo.length; c++)
            flagCache.push(prepare(templateCss, [c]));
        flags.html(flagCache.join(""));
        var logCache = [], dupeCount = 0, dupeMemory = "|||";
        for (var x = 0; x < logInfo.length; x++) {
            var dm = logInfo[x][1] + "|" + logInfo[x][2] + "|" + logInfo[x][3];
            if (dupeMemory !== dm) {
                if (dupeCount > 0)
                    logCache.push(prepare(templateLognotice, [logInfo[x - 1][1].toLowerCase(), modMap[logInfo[x - 1][2]], dupeCount]));
                dupeCount = 0;
                dupeMemory = dm;
                logCache.push(prepare(templateLogentry, [logInfo[x][1].toLowerCase(), modMap[logInfo[x][2]], logInfo[x][0], logInfo[x][1], logInfo[x][2], logInfo[x][3].split("  ").join("&nbsp ").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\n/g, "<br />")]));
            }
            else
                dupeCount++;
        }
        log.append(logCache.join(""));
        $("#modlink-r").on("click", removeFilter);
        $("#modlink-a").on("click", selectAll);
    }

    function prepare(str, arr) {
        var regex = /\{(\d)\}/g,
            match;
        while ((match = regex.exec(str)))
            str = str.replace(match[0], arr[match[1]]);
        return str;
    }
    function loadData() {
        try {
            stage = "loadData.Pre";
            var start = performance.now();
            parseData();
            renderData();
            var end = performance.now();
            $(".always").prepend("<div>Log processed in: " + (Math.round((end - start) * 100) / 100) + ' ms (<a id="viewraw" href="#">View raw</a>)</div><br />');
            $("#viewraw").on("click", function() {
                $("#dataraw").val($("#input").val());
                $("#popup-raw").fadeIn();
            });
            stage = "loadData.Post";
        }
        catch (err) {
            $("#output").html('<div id="log" class="color-red"><h1>Parsing failed!</h1>Parsing of the log failed, details follow.<br />&nbsp;<p>Stage: ' + stage + "</p>" + err + '<hr /><div id="rawlog"></div></div>');
            $("#rawlog").text($("#input").val());
        }
    }
    function getData() {
        $("#uploader").attr("data-text", "Loading...");
        $("#uploader").fadeIn();
        $.get("https://cors-anywhere.herokuapp.com/pastebin.com/raw/" + location.search.substring(1) + "/?nocache=" + Math.random(), function(data) {
            if (data.substring(0, 9) === "<!DOCTYPE") {
                $("#output").html('<div id="log" class="color-red"><h1>Captcha required!</h1>The pastebin server is asking for a captcha, but their API doesnt let us show it to you directly.<br />Instead, to finish saving the log, you need to <a href="https://pastebin.com/' + location.search.substring(1) + '" target="_blank">solve the captcha in a new tab</a>, once you have done so, reload this page.</div>');
            }
            else {
                $("#input").val(LZString.decompressFromUTF16(data) || data);
                loadData();
            }
            $("#uploader").fadeOut();
        });
    }
});

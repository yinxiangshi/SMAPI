/* globals $ */

var smapi = smapi || {};
var app;
smapi.modList = function (mods) {
    // init data
    var defaultStats = {
        total: 0,
        compatible: 0,
        workaround: 0,
        soon: 0,
        broken: 0,
        abandoned: 0,
        invalid: 0,
        smapi3_unknown: 0,
        smapi3_ok: 0,
        smapi3_broken: 0,
        smapi3_soon: 0
    };
    var data = {
        mods: mods,
        showAdvanced: false,
        visibleStats: $.extend({}, defaultStats),
        filters: {
            source: {
                open: {
                    label: "open",
                    id: "show-open-source",
                    value: true
                },
                closed: {
                    label: "closed",
                    id: "show-closed-source",
                    value: true
                }
            },
            status: {
                ok: {
                    label: "ok",
                    id: "show-status-ok",
                    value: true
                },
                optional: {
                    label: "optional",
                    id: "show-status-optional",
                    value: true
                },
                unofficial: {
                    label: "unofficial",
                    id: "show-status-unofficial",
                    value: true
                },
                workaround: {
                    label: "workaround",
                    id: "show-status-workaround",
                    value: true
                },
                broken: {
                    label: "broken",
                    id: "show-status-broken",
                    value: true
                },
                abandoned: {
                    label: "abandoned",
                    id: "show-status-abandoned",
                    value: true
                },
                obsolete: {
                    label: "obsolete",
                    id: "show-status-obsolete",
                    value: true
                }
            },
            download: {
                chucklefish: {
                    label: "Chucklefish",
                    id: "show-chucklefish",
                    value: true
                },
                moddrop: {
                    label: "ModDrop",
                    id: "show-moddrop",
                    value: true
                },
                nexus: {
                    label: "Nexus",
                    id: "show-nexus",
                    value: true
                },
                custom: {
                    label: "custom",
                    id: "show-custom",
                    value: true
                }
            },
            "SMAPI 3.0": {
                ok: {
                    label: "ready",
                    id: "show-smapi-3-ready",
                    value: true
                },
                soon: {
                    label: "soon",
                    id: "show-smapi-3-soon",
                    value: true
                },
                broken: {
                    label: "broken",
                    id: "show-smapi-3-broken",
                    value: true
                },
                unknown: {
                    label: "unknown",
                    id: "show-smapi-3-unknown",
                    value: true
                }
            }
        },
        search: ""
    };
    for (var i = 0; i < data.mods.length; i++) {
        var mod = mods[i];

        // set initial visibility
        mod.Visible = true;

        // set overall compatibility
        mod.LatestCompatibility = mod.BetaCompatibility || mod.Compatibility;

        // concatenate searchable text
        mod.SearchableText = [mod.Name, mod.AlternateNames, mod.Author, mod.AlternateAuthors, mod.Compatibility.Summary, mod.BrokeIn];
        if (mod.Compatibility.UnofficialVersion)
            mod.SearchableText.push(mod.Compatibility.UnofficialVersion);
        if (mod.BetaCompatibility) {
            mod.SearchableText.push(mod.BetaCompatibility.Summary);
            if (mod.BetaCompatibility.UnofficialVersion)
                mod.SearchableText.push(mod.BetaCompatibility.UnofficialVersion);
        }
        for (var p = 0; p < mod.ModPages; p++)
            mod.SearchableField.push(mod.ModPages[p].Text);
        mod.SearchableText = mod.SearchableText.join(" ").toLowerCase();
    }

    // init app
    app = new Vue({
        el: "#app",
        data: data,
        mounted: function() {
            // enable table sorting
            $("#mod-list").tablesorter({
                cssHeader: "header",
                cssAsc: "headerSortUp",
                cssDesc: "headerSortDown"
            });

            // put focus in textbox for quick search
            if (!location.hash)
                $("#search-box").focus();

            // jump to anchor (since table is added after page load)
            if (location.hash) {
                var row = $(location.hash).get(0);
                if (row)
                    row.scrollIntoView();
            }
        },
        methods: {
            /**
             * Update the visibility of all mods based on the current search text and filters.
             */
            applyFilters: function () {
                // get search terms
                var words = data.search.toLowerCase().split(" ");

                // apply criteria
                var stats = data.visibleStats = $.extend({}, defaultStats);
                for (var i = 0; i < data.mods.length; i++) {
                    var mod = data.mods[i];
                    mod.Visible = true;

                    // check filters
                    mod.Visible = this.matchesFilters(mod, words);
                    if (mod.Visible) {
                        stats.total++;
                        stats[this.getCompatibilityGroup(mod)]++;
                        stats["smapi3_" + mod.Smapi3Status]++;
                    }
                }
            },


            /**
             * Get whether a mod matches the current filters.
             * @param {object} mod The mod to check.
             * @param {string[]} searchWords The search words to match.
             * @returns {bool} Whether the mod matches the filters.
             */
            matchesFilters: function(mod, searchWords) {
                var filters = data.filters;

                // check source
                if (!filters.source.open.value && mod.SourceUrl)
                    return false;
                if (!filters.source.closed.value && !mod.SourceUrl)
                    return false;

                // check status
                var status = mod.LatestCompatibility.Status;
                if (filters.status[status] && !filters.status[status].value)
                    return false;

                // check SMAPI 3.0 compatibility
                if (filters["SMAPI 3.0"][mod.Smapi3Status] && !filters["SMAPI 3.0"][mod.Smapi3Status].value)
                    return false;

                // check download sites
                var ignoreSites = [];

                if (!filters.download.chucklefish.value)
                    ignoreSites.push("Chucklefish");
                if (!filters.download.moddrop.value)
                    ignoreSites.push("ModDrop");
                if (!filters.download.nexus.value)
                    ignoreSites.push("Nexus");
                if (!filters.download.custom.value)
                    ignoreSites.push("custom");

                if (ignoreSites.length) {
                    var anyLeft = false;
                    for (var i = 0; i < mod.ModPageSites.length; i++) {
                        if (ignoreSites.indexOf(mod.ModPageSites[i]) === -1) {
                            anyLeft = true;
                            break;
                        }
                    }

                    if (!anyLeft)
                        return false;
                }

                // check search terms
                for (var w = 0; w < searchWords.length; w++) {
                    if (mod.SearchableText.indexOf(searchWords[w]) === -1)
                        return false;
                }

                return true;
            },

            /**
             * Get a mod's compatibility group for mod metrics.
             * @param {object} mod The mod to check.
             * @returns {string} The compatibility group (one of 'compatible', 'workaround', 'soon', 'broken', 'abandoned', or 'invalid').
             */
            getCompatibilityGroup: function (mod) {
                var status = mod.LatestCompatibility.Status;
                switch (status) {
                    // obsolete
                    case "abandoned":
                    case "obsolete":
                        return "abandoned";

                    // compatible
                    case "ok":
                    case "optional":
                        return "compatible";

                    // workaround
                    case "workaround":
                    case "unofficial":
                        return "workaround";

                    // soon/broken
                    case "broken":
                        if (mod.SourceUrl)
                            return "soon";
                        else
                            return "broken";

                    default:
                        return "invalid";
                }
            }
        }
    });
    app.applyFilters();
};

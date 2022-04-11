/* globals $, Vue */

/**
 * The global SMAPI module.
 */
var smapi = smapi || {};

/**
 * The Vue app for the current page.
 * @type {Vue}
 */
var app;

// Use a scroll event to apply a sticky effect to the filters / pagination
// bar. We can't just use "position: sticky" due to how the page is structured
// but this works well enough.
$(function () {
    let sticking = false;

    document.addEventListener("scroll", function () {
        const filters = document.getElementById("filters");
        const holder = document.getElementById("filterHolder");
        if (!filters || !holder)
            return;

        const offset = holder.offsetTop;
        const shouldStick = window.pageYOffset > offset;
        if (shouldStick === sticking)
            return;

        sticking = shouldStick;
        if (sticking) {
            holder.style.marginBottom = `calc(1em + ${filters.offsetHeight}px)`;
            filters.classList.add("sticky");
        }
        else {
            filters.classList.remove("sticky");
            holder.style.marginBottom = "";
        }
    });
});

/**
 * Initialize a log parser view on the current page.
 * @param {object} state The state options to use.
 * @returns {void}
 */
smapi.logParser = function (state) {
    if (!state)
        state = {};

    // internal helpers
    const helpers = {
        /**
         * Get a handler which invokes the callback after a set delay, resetting the delay each time it's called.
         * @param {(...*) => void} action The callback to invoke when the delay ends.
         * @param {number} delay The number of milliseconds to delay the action after each call.
         * @returns {() => void}
         */
        getDebouncedHandler(action, delay) {
            let timeoutId = null;

            return function () {
                clearTimeout(timeoutId);

                const args = arguments;
                const self = this;

                timeoutId = setTimeout(
                    function () {
                        action.apply(self, args);
                    },
                    delay
                );
            }
        },

        /**
         * Escape regex special characters in the given string.
         * @param {string} text
         * @returns {string}
         */
        escapeRegex(text) {
            return text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
        },

        /**
         * Format a number for the user's locale.
         * @param {number} value The number to format.
         * @returns {string}
         */
        formatNumber(value) {
            const formatter = window.Intl && Intl.NumberFormat && new Intl.NumberFormat();
            return formatter && formatter.format
                ? formatter.format(value)
                : `${value}`;
        },

        /**
         * Convert an array of boolean values into a bitmap.
         * @param {Boolean[]} value An array of boolean values
         * @returns {BigInt}
         */
        toBitmap(value) {
            let result = BigInt(0);
            if (!Array.isArray(value))
                return value ? BigInt(1) : BigInt(0);

            for (let i = 0; i < value.length; i++) {
                if (value[i])
                    result += BigInt(2) ** BigInt(value.length - i - 1);
            }

            return result;
        },

        /**
         * Convert a bitmap into an array of boolean values.
         * @param {BigInt} value The bitmap
         * @param {Number} length The expected length of the result
         * @returns {Boolean[]}
         */
        fromBitmap(value, length = -1) {
            if (typeof value != "bigint")
                value = "";
            else
                value = value.toString(2);

            const result = [];
            while (length > value.length) {
                result.push(false);
                length--;
            }

            for (let i = 0; i < value.length; i++) {
                result.push(value[i] === "1" ? true : false);
            }

            return result;
        },

        b64ToBigInt(value) {
            const bin = atob(value);
            const hex = [];

            for (let i = 0; i < bin.length; i++) {
                let h = bin.charCodeAt(i).toString(16);
                if (h.length % 2) h = `0${h}`;
                hex.push(h);
            }

            return BigInt(`0x${hex.join('')}`);
        },

        bigIntTo64(value) {
            let hex = value.toString(16);
            if (hex.length % 2) hex = `0${hex}`;

            const result = [];
            for (let i = 0; i < hex.length; i += 2) {
                const val = parseInt(hex.slice(i, i + 2), 16);
                result.push(String.fromCharCode(val));
            }

            return btoa(result.join(''));
        },

        b64ToUrl(value) {
            return value.replace(/\//g, '_').replace(/=/g, '-').replace(/\+/g, '.');
        },

        urlTob64(value) {
            return value.replace(/_/g, '/').replace(/-/g, '=').replace(/\./g, '+');
        },

        toUrlBitmap(value) {
            return helpers.b64ToUrl(helpers.bigIntTo64(helpers.toBitmap(value)));
        },

        fromUrlBitmap(value, length = -1) {
            return helpers.fromBitmap(helpers.b64ToBigInt(helpers.urlTob64(value)), length);
        }

    };

    // internal event handlers
    const handlers = {
        /**
         * Method called when the user clicks a log line to toggle the visibility of a section. Binding methods is problematic with functional components so we just use the `data-section` parameter and our global reference to the app.
         * @param {any} event
         * @returns {false}
         */
        clickLogLine(event) {
            app.toggleSection(event.currentTarget.dataset.section);
            event.preventDefault();
            return false;
        },

        /**
         * Navigate to the previous page of messages in the log.
         * @returns {void}
         */
        prevPage() {
            app.prevPage();
        },

        /**
         * Navigate to the next page of messages in the log.
         * @returns {void}
         */
        nextPage() {
            app.nextPage();
        },

        /**
         * Handle a click on a page number element.
         * @param {number | Event} event
         * @returns {void}
         */
        changePage(event) {
            if (typeof event === "number")
                app.changePage(event);
            else if (event) {
                const page = parseInt(event.currentTarget.dataset.page);
                if (!isNaN(page) && isFinite(page))
                    app.changePage(page);
            }
        }
    };

    // internal filter counts
    const stats = state.stats = {
        modsShown: 0,
        modsHidden: 0
    };

    function updateModFilters() {
        // counts
        stats.modsShown = 0;
        stats.modsHidden = 0;
        for (let key in state.showMods) {
            if (state.showMods.hasOwnProperty(key)) {
                if (state.showMods[key])
                    stats.modsShown++;
                else
                    stats.modsHidden++;
            }
        }
    }

    // load raw log data
    {
        const dataElement = document.querySelector(state.dataElement);
        state.data = JSON.parse(dataElement.textContent.trim());
        dataElement.remove(); // let browser unload the data element since we won't need it anymore
    }

    // preprocess data for display
    state.messages = state.data.messages || [];
    if (state.messages.length) {
        const levels = state.data.logLevels;
        const sections = state.data.sections;
        const modSlugs = state.data.modSlugs;

        for (let i = 0, length = state.messages.length; i < length; i++) {
            const message = state.messages[i];

            // add unique ID
            message.id = i;

            // add display values
            message.LevelName = levels[message.Level];
            message.SectionName = sections[message.Section];
            message.ModSlug = modSlugs[message.Mod] || message.Mod;

            // For repeated messages, since our <log-line /> component
            // can't return two rows, just insert a second message
            // which will display as the message repeated notice.
            if (message.Repeated > 0 && !message.isRepeated) {
                const repeatNote = {
                    id: i + 1,
                    Level: message.Level,
                    Section: message.Section,
                    Mod: message.Mod,
                    Repeated: message.Repeated,
                    isRepeated: true
                };

                state.messages.splice(i + 1, 0, repeatNote);
                length++;
            }

            // let Vue know the message won't change, so it doesn't need to monitor it
            Object.freeze(message);
        }
    }
    Object.freeze(state.messages);

    // set local time started
    if (state.logStarted)
        state.localTimeStarted = ("0" + state.logStarted.getHours()).slice(-2) + ":" + ("0" + state.logStarted.getMinutes()).slice(-2);

    // add the properties we're passing to Vue
    state.totalMessages = state.messages.length;
    state.filterText = "";
    state.filterRegex = "";
    state.showContentPacks = true;
    state.useHighlight = true;
    state.useRegex = false;
    state.useInsensitive = true;
    state.useWord = false;
    state.perPage = 1000;
    state.page = 1;

    // load saved values, if any
    if (localStorage.settings) {
        try {
            const saved = JSON.parse(localStorage.settings);

            state.showContentPacks = saved.showContentPacks ?? state.showContentPacks;
            state.useHighlight = saved.useHighlight ?? state.useHighlight;
            state.useRegex = saved.useRegex ?? state.useRegex;
            state.useInsensitive = saved.useInsensitive ?? state.useInsensitive;
            state.useWord = saved.useWord ?? state.useWord;
        }
        catch (error) {
            // ignore settings if invalid
        }
    }

    // add a number formatter so our numbers look nicer
    Vue.filter("number", handlers.formatNumber);

    // Strictly speaking, we don't need this. However, due to the way our
    // Vue template is living in-page the browser is "helpful" and moves
    // our <log-line />s outside of a basic <table> since obviously they
    // aren't table rows and don't belong inside a table. By using another
    // Vue component, we avoid that.
    Vue.component("log-table", {
        functional: true,
        render: function (createElement, context) {
            return createElement(
                "table",
                {
                    attrs: {
                        id: "log"
                    }
                },
                context.children
            );
        }
    });

    // The <filter-stats /> component draws a nice message under the filters
    // telling a user how many messages match their filters, and also expands
    // on how many of them they're seeing because of pagination.
    Vue.component("filter-stats", {
        functional: true,
        render: function (createElement, context) {
            const props = context.props;
            return createElement(
                "div",
                { class: "stats" },
                [
                    "showing ",
                    createElement("strong", helpers.formatNumber(props.start + 1)),
                    " to ",
                    createElement("strong", helpers.formatNumber(props.end)),
                    " of ",
                    createElement("strong", helpers.formatNumber(props.filtered)),
                    " (total: ",
                    createElement("strong", helpers.formatNumber(props.total)),
                    ")"
                ]
            );
        }
    });

    // Next up we have <pager /> which renders the pagination list. This has a
    // helper method to make building the list of links easier.
    function addPageLink(page, links, visited, createElement, currentPage) {
        if (visited.has(page))
            return;

        if (page > 1 && !visited.has(page - 1))
            links.push(" … ");

        visited.add(page);
        links.push(createElement(
            "span",
            {
                class: page === currentPage ? "active" : null,
                attrs: {
                    "data-page": page
                },
                on: {
                    click: handlers.changePage
                }
            },
            helpers.formatNumber(page)
        ));
    }

    Vue.component("pager", {
        functional: true,
        render: function (createElement, context) {
            const props = context.props;
            if (props.pages <= 1)
                return null;

            const visited = new Set();
            const pageLinks = [];

            for (let i = 1; i <= 2; i++)
                addPageLink(i, pageLinks, visited, createElement, props.page);

            for (let i = props.page - 2; i <= props.page + 2; i++) {
                if (i >= 1 && i <= props.pages)
                    addPageLink(i, pageLinks, visited, createElement, props.page);
            }

            for (let i = props.pages - 2; i <= props.pages; i++) {
                if (i >= 1)
                    addPageLink(i, pageLinks, visited, createElement, props.page);
            }

            return createElement(
                "div",
                { class: "pager" },
                [
                    createElement(
                        "span",
                        {
                            class: props.page <= 1 ? "disabled" : null,
                            on: {
                                click: handlers.prevPage
                            }
                        },
                        "Prev"
                    ),
                    " ",
                    "Page ",
                    helpers.formatNumber(props.page),
                    " of ",
                    helpers.formatNumber(props.pages),
                    " ",
                    createElement(
                        "span",
                        {
                            class: props.page >= props.pages ? "disabled" : null,
                            on: {
                                click: handlers.nextPage
                            }
                        },
                        "Next"
                    ),
                    createElement("div", {}, pageLinks)
                ]
            );
        }
    });

    // Our <log-line /> functional component draws each log line.
    Vue.component("log-line", {
        functional: true,
        props: {
            showScreenId: {
                type: Boolean,
                required: true
            },
            message: {
                type: Object,
                required: true
            },
            highlight: {
                type: Boolean,
                required: false
            }
        },
        render: function (createElement, context) {
            const message = context.props.message;
            const level = message.LevelName;

            if (message.isRepeated)
                return createElement(
                    "tr",
                    {
                        class: [
                            "mod",
                            level,
                            "mod-repeat"
                        ]
                    },
                    [
                        createElement(
                            "td",
                            {
                                attrs: {
                                    colspan: context.props.showScreenId ? 4 : 3
                                }
                            },
                            ""
                        ),
                        createElement("td", `repeats ${message.Repeated} times`)
                    ]
                );

            const events = {};
            let toggleMessage;
            if (message.IsStartOfSection) {
                const visible = message.SectionName && window.app && app.sectionsAllow(message.SectionName);
                events.click = handlers.clickLogLine;
                toggleMessage = visible
                    ? "This section is shown. Click here to hide it."
                    : "This section is hidden. Click here to show it.";
            }

            let text = message.Text;
            const filter = window.app && app.filterRegex;
            if (text && filter && context.props.highlight) {
                text = [];
                let match;
                let consumed = 0;
                let index = 0;
                filter.lastIndex = -1;

                // Our logic to highlight the text is a bit funky because we
                // want to group consecutive matches to avoid a situation
                // where a ton of single characters are in their own elements
                // if the user gives us bad input.

                while (true) {
                    match = filter.exec(message.Text);
                    if (!match)
                        break;

                    // Do we have an area of non-matching text? This
                    // happens if the new match's index is further
                    // along than the last index.
                    if (match.index > index) {
                        // Alright, do we have a previous match? If
                        // we do, we need to consume some text.
                        if (consumed < index)
                            text.push(createElement("strong", {}, message.Text.slice(consumed, index)));

                        text.push(message.Text.slice(index, match.index));
                        consumed = match.index;
                    }

                    index = match.index + match[0].length;
                }

                // Add any trailing text after the last match was found.
                if (consumed < message.Text.length) {
                    if (consumed < index)
                        text.push(createElement("strong", {}, message.Text.slice(consumed, index)));

                    if (index < message.Text.length)
                        text.push(message.Text.slice(index));
                }
            }

            return createElement(
                "tr",
                {
                    class: [
                        "mod",
                        level,
                        message.IsStartOfSection ? "section-start" : null
                    ],
                    attrs: {
                        "data-section": message.SectionName
                    },
                    on: events
                },
                [
                    createElement("td", message.Time),
                    context.props.showScreenId ? createElement("td", message.ScreenId) : null,
                    createElement("td", level.toUpperCase()),
                    createElement(
                        "td",
                        {
                            attrs: {
                                "data-title": message.Mod
                            }
                        },
                        message.Mod
                    ),
                    createElement(
                        "td",
                        [
                            createElement(
                                "span",
                                { class: "log-message-text" },
                                text
                            ),
                            message.IsStartOfSection
                                ? createElement(
                                    "span",
                                    { class: "section-toggle-message" },
                                    [
                                        " ",
                                        toggleMessage
                                    ]
                                )
                                : null
                        ]
                    )
                ]
            );
        }
    });

    // init app
    app = new Vue({
        el: "#output",
        data: state,
        computed: {
            anyModsHidden: function () {
                return stats.modsHidden > 0;
            },
            anyModsShown: function () {
                return stats.modsShown > 0;
            },
            showScreenId: function () {
                return this.data.screenIds.length > 1;
            },

            // Maybe not strictly necessary, but the Vue template is being
            // weird about accessing data entries on the app rather than
            // computed properties.
            hideContentPacks: function () {
                return !state.showContentPacks;
            },

            // Filter messages for visibility.
            filterUseRegex: function () {
                return state.useRegex;
            },
            filterInsensitive: function () {
                return state.useInsensitive;
            },
            filterUseWord: function () {
                return state.useWord;
            },
            shouldHighlight: function () {
                return state.useHighlight;
            },

            filteredMessages: function () {
                if (!state.messages)
                    return [];

                //const start = performance.now();
                const filtered = [];

                let total = 0;

                // This is slightly faster than messages.filter(), which is
                // important when working with absolutely huge logs.
                for (let i = 0, length = state.messages.length; i < length; i++) {
                    const msg = state.messages[i];
                    if (!this.filtersAllow(msg.ModSlug, msg.LevelName))
                        continue;

                    if (this.filterRegex) {
                        const text = msg.Text || (i > 0 ? state.messages[i - 1].Text : null);
                        this.filterRegex.lastIndex = -1;
                        if (!text || !this.filterRegex.test(text))
                            continue;
                    }

                    total++;

                    if (msg.SectionName && !msg.IsStartOfSection && !this.sectionsAllow(msg.SectionName))
                        continue;

                    filtered.push(msg);
                }

                filtered.total = total;

                Object.freeze(filtered);

                //const end = performance.now();
                //console.log(`applied ${(this.useRegex ? "regex" : "text")} filter '${this.filterRegex}' in ${end - start}ms`);

                return filtered;
            },

            // And the rest are about pagination.
            start: function () {
                return (this.page - 1) * state.perPage;
            },
            end: function () {
                return this.start + this.visibleMessages.length;
            },
            totalPages: function () {
                return Math.ceil(this.filteredMessages.length / state.perPage);
            },
            // 
            visibleMessages: function () {
                if (this.totalPages <= 1)
                    return this.filteredMessages;

                const start = this.start;
                const end = start + state.perPage;

                return this.filteredMessages.slice(start, end);
            }
        },
        created: function () {
            window.addEventListener("popstate", () => this.loadFromUrl());
            this.loadFromUrl();
        },
        methods: {
            loadFromUrl: function () {
                const params = new URL(location).searchParams;
                if (params.has("PerPage")) {
                    const perPage = parseInt(params.get("PerPage"));
                    if (!isNaN(perPage) && isFinite(perPage) && perPage > 0)
                        state.perPage = perPage;
                }

                if (params.has("Page")) {
                    const page = parseInt(params.get("Page"));
                    if (!isNaN(page) && isFinite(page) && page > 0)
                        this.page = page;
                }

                let updateFilter = false;

                if (params.has("Filter")) {
                    state.filterText = params.get("Filter");
                    updateFilter = true;
                }

                if (params.has("FilterMode")) {
                    const values = helpers.fromUrlBitmap(params.get("FilterMode"), 3);
                    state.useRegex = values[0];
                    state.useInsensitive = values[1];
                    state.useWord = values[2];
                    updateFilter = true;
                }

                if (params.has("Mods")) {
                    const keys = Object.keys(this.showMods);
                    const value = params.get("Mods");
                    const values = value === "all" ? true : value === "none" ? false : helpers.fromUrlBitmap(value, keys.length);

                    for (let i = 0; i < keys.length; i++) {
                        this.showMods[keys[i]] = Array.isArray(values) ? values[i] : values;
                    }

                    updateModFilters();
                }

                if (params.has("Levels")) {
                    const keys = Object.keys(this.showLevels);
                    const values = helpers.fromUrlBitmap(params.get("Levels"), keys.length);

                    for (let i = 0; i < keys.length; i++) {
                        this.showLevels[keys[i]] = values[i];
                    }
                }

                if (params.has("Sections")) {
                    const keys = Object.keys(this.showSections);
                    const values = helpers.fromUrlBitmap(params.get("Levels"), keys.length);

                    for (let i = 0; i < keys.length; i++) {
                        this.showSections[keys[i]] = values[i];
                    }
                }

                if (updateFilter)
                    this.updateFilterText();
            },

            // Whenever the page state changed, replace the current page URL. Using
            // replaceState rather than pushState to avoid filling the tab history
            // with tons of useless history steps the user probably doesn't
            // really care about.
            updateUrl: function () {
                const url = new URL(location);
                url.searchParams.set("Page", state.page);
                url.searchParams.set("PerPage", state.perPage);

                url.searchParams.set("Mods", stats.modsHidden == 0 ? "all" : stats.modsShown == 0 ? "none" : helpers.toUrlBitmap(Object.values(this.showMods)));
                url.searchParams.set("Levels", helpers.toUrlBitmap(Object.values(this.showLevels)));
                url.searchParams.set("Sections", helpers.toUrlBitmap(Object.values(this.showSections)));

                if (state.filterText && state.filterText.length) {
                    url.searchParams.set("Filter", state.filterText);
                    url.searchParams.set("FilterMode", helpers.toUrlBitmap([state.useRegex, state.useInsensitive, state.useWord]));
                } else {
                    url.searchParams.delete("Filter");
                    url.searchParams.delete("FilterMode");
                }

                window.history.replaceState(null, document.title, url.toString());
            },

            toggleLevel: function (id) {
                if (!state.enableFilters)
                    return;

                this.showLevels[id] = !this.showLevels[id];
                this.updateUrl();
            },

            toggleContentPacks: function () {
                state.showContentPacks = !state.showContentPacks;
                this.saveSettings();
            },

            toggleFilterUseRegex: function () {
                state.useRegex = !state.useRegex;
                this.saveSettings();
                this.updateFilterText();
            },

            toggleFilterInsensitive: function () {
                state.useInsensitive = !state.useInsensitive;
                this.saveSettings();
                this.updateFilterText();
            },

            toggleFilterWord: function () {
                state.useWord = !state.useWord;
                this.saveSettings();
                this.updateFilterText();
            },

            toggleHighlight: function () {
                state.useHighlight = !state.useHighlight;
                this.saveSettings();
            },

            prevPage: function () {
                if (this.page <= 1)
                    return;
                this.page--;
                this.updateUrl();
            },

            nextPage: function () {
                if (this.page >= this.totalPages)
                    return;
                this.page++;
                this.updateUrl();
            },

            changePage: function (page) {
                if (page < 1 || page > this.totalPages)
                    return;
                this.page = page;
                this.updateUrl();
            },

            // Persist settings into localStorage for use the next time
            // the user opens a log.
            saveSettings: function () {
                localStorage.settings = JSON.stringify({
                    showContentPacks: state.showContentPacks,
                    useRegex: state.useRegex,
                    useInsensitive: state.useInsensitive,
                    useWord: state.useWord,
                    useHighlight: state.useHighlight
                });
            },

            // We don't want to update the filter text often, so use a debounce with
            // a quarter second delay. We basically always build a regular expression
            // since we use it for highlighting, and it also make case insensitivity
            // much easier.
            updateFilterText: helpers.getDebouncedHandler(
                function () {
                    let text = state.filterText;
                    if (!text || !text.length) {
                        this.filterText = "";
                        this.filterRegex = null;
                    }
                    else {
                        if (!state.useRegex)
                            text = helpers.escapeRegex(text);
                        this.filterRegex = new RegExp(
                            state.useWord ? `\\b${text}\\b` : text,
                            state.useInsensitive ? "ig" : "g"
                        );
                    }

                    this.updateUrl();
                },
                250
            ),

            toggleMod: function (id) {
                if (!state.enableFilters)
                    return;

                const curShown = this.showMods[id];

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
                this.updateUrl();
            },

            toggleSection: function (name) {
                if (!state.enableFilters)
                    return;

                this.showSections[name] = !this.showSections[name];
                this.updateUrl();
            },

            showAllMods: function () {
                if (!state.enableFilters)
                    return;

                for (let key in this.showMods) {
                    if (this.showMods.hasOwnProperty(key)) {
                        this.showMods[key] = true;
                    }
                }
                updateModFilters();
                this.updateUrl();
            },

            hideAllMods: function () {
                if (!state.enableFilters)
                    return;

                for (let key in this.showMods) {
                    if (this.showMods.hasOwnProperty(key)) {
                        this.showMods[key] = false;
                    }
                }
                updateModFilters();
                this.updateUrl();
            },

            filtersAllow: function (modId, level) {
                return this.showMods[modId] !== false && this.showLevels[level] !== false;
            },

            sectionsAllow: function (section) {
                return this.showSections[section] !== false;
            }
        }
    });

    /**********
    ** Upload form
    *********/
    const input = $("#input");
    if (input.length) {
        // file upload
        smapi.fileUpload({
            chooseFileLink: $("#choose-file-link"),
            chooseFileInput: $("#inputFile"),
            contentArea: input,
            submitButton: $("#submit")
        });
    }
};

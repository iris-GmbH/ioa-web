function refreshDownloadType() {
    finalData.fileExType = $('.download-formats option:selected').data('filext');
    finalData.requestType = $('.download-formats option:selected').val();
}

function showModalAccountMessage() {
    var mymodal = $('#mainModal');
    var infoString = $.i18n('$modal_accountName') + " - " + loginData.name + "\n\r"
        + $.i18n('$modal_accountLongName') + " - " + loginData.longName + "\n\r"
        + $.i18n('$modal_accountShortName') + " - " + loginData.shortName + "\n\r"
        + $.i18n('$modal_accountMail') + " - " + loginData.email + "\n\r"
        + $.i18n('$modal_accountTimeZone') + " - " + loginData.timeZone + "\n\r"
        + $.i18n('$modal_accountLanguage') + " - " + loginData.language + "";
    mymodal.find('.modal-body').text(infoString);
    mymodal.find('.modal-title').text($.i18n('$modal_accountTitle'));
    mymodal.modal('show');
}

/* Custom Responsiveness over javascript, because used ui frameworks dont want to work other way */
function getCalendarHeight() {
    var calendarOffset = 268;
    var calendarMinimalOffset = 672; //fullcalendar minimal length before scrollbars come

    var finalReturn = $(window).height() - calendarOffset;

    if ($(window).height() <= calendarMinimalOffset) { return calendarMinimalOffset; }
    return finalReturn;
}

function getListHeight() {
    var listOffset = 466;
    var listMinimalOffset = 672;

    var finalReturn = $(window).height() - listOffset;

    if ($(window).height() <= listMinimalOffset) { return listOffset - 4; }
    return finalReturn / 1.2;
}

function ajaxSuccess() {

    $('.main').removeClass('scale-to-background');
    $('#modal-login div').fadeOut(123);
    $("#modal-small").hide().animate({ easing: 'easeOutQuint', duration: 300, queue: false });

    $('.main').animate({
        opacity: 1,
        easing: 'easeOutQuint',
        width: "100%",
        queue: false,
        height: "100%",
        top: 0
    }, function () {
        // Animation complete.

        $('.selectpicker').selectpicker(); //init. filepicker with framework
        $("#div_loading").hide();
        $('.searchable').multiSelect({
            selectableHeader: "<input type='text' class='search-input' placeholder='&#xF002;'>",
            selectionHeader: "<input type='text' class='search-input' placeholder='&#xF002;'>",
            selectableFooter: "<div data-i18n='$vehicle_picker_search_from' class='custom-header'>...</div>",
            selectionFooter: "<div data-i18n='$vehicle_picker_search_to' class='custom-header'>...</div>",
            afterInit: function (ms) {
                var that = this,
                    $selectableSearch = that.$selectableUl.prev(),
                    $selectionSearch = that.$selectionUl.prev(),
                    selectableSearchString = '#' + that.$container.attr('id') + ' .ms-elem-selectable:not(.ms-selected)',
                    selectionSearchString = '#' + that.$container.attr('id') + ' .ms-elem-selection.ms-selected';

                that.qs1 = $selectableSearch.quicksearch(selectableSearchString)
                    .on('keydown', function (e) {
                        if (e.which === 40) {
                            that.$selectableUl.focus();
                            return false;
                        }
                    });

                that.qs2 = $selectionSearch.quicksearch(selectionSearchString)
                    .on('keydown', function (e) {
                        if (e.which === 40) {
                            that.$selectionUl.focus();
                            return false;
                        }
                    });
                //Reinitialize picked values after init phase, because firefox remembers the picked ones when softrefresh occurs...
                finalData.deviceList = $('#device-list').val();

                //Inject select/deselect all button into middle of container
                $(".ms-selectable").after('<div class="select-flip"><i id="flipItemList" class="fa fa-2x fa-exchange" aria-hidden="true"></i></div>');
                $('#flipItemList').click(function () {
                    if (allSelected === false) {
                        allSelected = true;
                        $('.searchable').multiSelect('select_all');
                    }
                    else {
                        allSelected = false;
                        $('.searchable').multiSelect('deselect_all');
                    }
                });
            },
            afterSelect: function () {
                this.qs1.cache();
                this.qs2.cache();
                finalData.deviceList = $('#device-list').val(); //Reinit Reference
            },
            afterDeselect: function () {
                this.qs1.cache();
                this.qs2.cache();
                finalData.deviceList = $('#device-list').val(); //Reinit Reference
            }
        });

        $(window).resize(function () {

            //callendar resize
            $('#calendar').fullCalendar('option', 'height', getCalendarHeight());

            //multiselect resize
            var listCollection = $(".ms-list");
            listCollection.each(function (i, obj) {
                obj.setAttribute('style', 'height: ' + getListHeight() + 'px; !important');
            });

        });

        updateText(); //Update the Partitial View strings
        $('#calendar').fullCalendar({
            defaultDate: '2017-01-01',
            locale: $('.language option:selected').val(),
            longPressDelay: 50,
            height: getCalendarHeight(),
            eventLimit: true,
            header: {
                left: 'prev,next',
                center: 'title',
                right: 'resetButton,today'
            },
            defaultView: 'month',
            customButtons: {
                resetButton: {
                    text: $.i18n('$mainapp_calendar_reset_button'),
                    class: "btn-danger",
                    click: function (text) {
                        $('#calendar').fullCalendar('removeEventSources');
                        requestArray = [];
                        finalData.dateList = requestArray; //Reinit the reference
                        showModalMessage($.i18n('$modal_calendar_reset_text'));
                    }
                }
            },
            events: [],
            selectable: true,
            selectHelper: true,
            select: function (start, end, jsEvent, view) {
                var futureCheck = moment().diff(start, 'days', true); //true = not rounded value (because of timezones in timestamps...)
                var futureEndCloned = end.clone(); //Becasue of fullcalendar exclusive momentjs dates...
                var futureCheckEnd = moment().diff(futureEndCloned.subtract(1, "days"), 'days', true);

                //Allow one day in future because some user want to access data from a timezone that lies in future
                var dayFilter = -1;  // 0 to completely disable future days
                if (futureCheck < dayFilter || futureCheckEnd < dayFilter) {
                    //Ignore Days in future
                    $('#calendar').fullCalendar('unselect');
                    showModalMessage($.i18n('$modal_calendar_date_futurelimit'));
                    return false;
                }

                var startC = moment(start);
                var endC = moment(end);
                var check = endC.diff(startC, "days");
                var checkedRequested = requestArray.length + check;

                //If saved requests are over 31 (or other def. value) or the sum of all selected + newest select range are over 31
                if (requestArray.length <= dayLimit && checkedRequested <= dayLimit) {

                    for (var m = moment(start); m.isBefore(end); m.add(1, 'days')) {
                        requestArray.push(new moment(m));
                    }

                    $("#calendar").fullCalendar('addEventSource', [{
                        start: start,
                        end: end,
                        //color: "blue",
                        rendering: 'background', //green background for selected feedback
                        block: true //blocks field to be reselected
                    }]);

                } else {
                    showModalMessage($.i18n('$modal_calendar_date_limit'));
                }
                $("#calendar").fullCalendar("unselect");
            },
            selectOverlap: function (event) {
                $("#calendar").fullCalendar('removeEventSource', event.source);

                //Remove selected daterange (event triggers only when selected Dates overlaps)
                for (var m = moment(event.start); m.isBefore(event.end); m.add(1, 'days')) {
                    for (var i = 0; i < finalData.dateList.length; i++) {
                        if (finalData.dateList[i].isSame(m)) {
                            finalData.dateList.splice(i, 1);
                            break;
                        }
                    }
                }

                return !event.block;
            }
        });

        //Fullcalendar mouse cursor bugfix
        $(window).mouseup(function () {
            $('body').removeClass('fc-not-allowed');
        });

        //Style list fix
        $(".ms-list").each(function (i, obj) {
            obj.setAttribute('style', 'height: ' + getListHeight() + 'px; !important');
        });

        //Force the actual client month to be loaded as first
        $('#calendar').fullCalendar('today'); 
        refreshDownloadType();

    });
}

function ajaxFailure(ajaxContext) {
    var response = ajaxContext.responseText;
    showModalMessage($.i18n('$modal_ajax_error') + "Code - [" + ajaxContext.ErrorCode + "] " + response);
}

function switchUiLockState() {
    if ($('#downloadButton').prop('disabled') || $('#downloadButton').prop('disabled')) {

        $('#downloadButton').prop('disabled', false);
        $('#formatListId').prop('disabled', false);
        $("body").css("cursor", "default");
        return false;
    } else {
        $('#downloadButton').prop('disabled', true);
        $('#formatListId').prop('disabled', true);
        $("body").css("cursor", "progress");
        return true;
    }
}

function sendRequesttoServer(selectedFormat) {
    finalData.requestType = selectedFormat;    
    switchUiLockState();

    if (finalData.dateList.length > 0 && finalData.deviceList !== null) {
        finalData.dateList.sortBy(function (o) { return o.date; });
        var firstDate = new Date(finalData.dateList[0]);
        firstDate = firstDate.getFullYear() + '/' + (firstDate.getMonth() + 1) + '/' + firstDate.getDate();
        var lastDate = new Date(finalData.dateList[finalData.dateList.length - 1]);
        lastDate = lastDate.getFullYear() + '/' + (lastDate.getMonth() + 1) + '/' + lastDate.getDate();

        xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function () {

            if (xhttp.readyState === 4 && xhttp.status === 200) {
                //uses blob.js + filesaver.js (for legacy support)
                var blob = new Blob([xhttp.response], { type: xhttp.response.type });
                saveAs(blob, loginData.name + "-" + "(" + firstDate + " - " + lastDate + ")-" + finalData.fileExType + ".zip");
                switchUiLockState();
            }

            //returned HttpCode is not OK
            if (xhttp.readyState === 4 && xhttp.status !== 200) {

                if (xhttp.status === 408)
                    showModalMessage($.i18n('$modal_session_expired'));
                else
                    showModalMessage($.i18n('$modal_ajax_error') + "\nCode: " + xhttp.status);

                //Execute refresh callback only when the modal is gonna be closed by user
                $("#mainModal").visibilityChanged({
                    callback: function (element, visible) {
                        if (visible === false) location.href = "/";
                    },
                    runOnLoad: false,
                    frequency: 100
                });
            }
        };
        // Post data to URL which handles post request
        xhttp.open("POST", "/Home/DataRequest");
        xhttp.setRequestHeader("Content-Type", "application/json");
        xhttp.responseType = 'blob';
        xhttp.send(JSON.stringify(finalData));

    } else {
        switchUiLockState();
        showModalMessage($.i18n('$modal_vehiclepicker_all_notselected'));
    }
}

//sortBy function for Dates using Schwartzian transform
function sortBy(f) {
    for (var i = this.length; i;) {
        var o = this[--i];
        this[i] = [].concat(f.call(o, o, i), o);
    }
    this.sort(function (a, b) {
        for (var i = 0, len = a.length; i < len; ++i) {
            if (a[i] !== b[i]) return a[i] < b[i] ? -1 : 1;
        }
        return 0;
    });
    for (var j = this.length; j;) {
        this[--j] = this[j][this[j].length - 1];
    }
    return this;
}

$(function () {
    if (typeof Object.defineProperty === 'function') {
        try { Object.defineProperty(Array.prototype, 'sortBy', { value: sortBy }); } catch (e) { console.log(e.toString()); }
    }
    if (!Array.prototype.sortBy) Array.prototype.sortBy = sortBy;

    $('.language').on('change', function () {
        if (this.value) { $('#calendar').fullCalendar('option', 'locale', this.value); }
    });

    $('#accountButton').click(function () { showModalAccountMessage(); });
    $('.download-formats').on('change keyup', refreshDownloadType);

    //Show extra Container after successful login
    $('#header-meta').removeClass('hidden');

    //Set account name in "welcome" box
    $("#headerInfoName").text(loginData.longName);
});
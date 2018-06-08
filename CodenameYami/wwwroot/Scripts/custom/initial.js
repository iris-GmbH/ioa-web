function ajaxBegin() {
    $('#loginButton').prop("disabled", true); //Disable to prevent multiple login spam
    $("#loginModal").show().animate({ easing: 'easeOutQuint', duration: 300, queue: true });
    $('.main').addClass('scale-to-background');//.done(function() { //activate scale effect
    $("#loginModal").animate({ opacity: 1 }, { duration: 300, queue: true });
}

function ajaxFailure(result, ref) {
    //result.status (int) & result.statusText (string)
    if (result.status === 403) $("#connectFailedString").attr("data-i18n", "$login_forbidden");
    else $("#connectFailedString").attr("data-i18n", "$login_failed");

    updateText();
    $("#connectFailedCodeString").text("Code: " + result.status); //Create httpcode text
    $("#loginModal").hide().animate({ opacity: 0 }, { easing: 'easeOutQuint', duration: 0, queue: true });
    $("#loginModalFailed").show().animate({ easing: 'easeOutQuint', duration: 300, queue: true });
    $("#loginModalFailed").animate({ opacity: 1 }, { duration: 300, queue: true });
}

function showLogin() {
    $('.main').removeClass('scale-to-background');
    $("#loginModalFailed").hide().animate({ opacity: 0 }, { easing: 'easeOutQuint', duration: 0, queue: true });
    $("#connectFailedCodeString").text(""); //Clear httpcode text
    $('#loginButton').prop("disabled", false); //Reactivate login form
}

function showModalMessage(message) {
    var myModal = $('#mainModal');
    myModal.find('.modal-body').text(message);
    myModal.find('.modal-title').text($.i18n('$modal_messagetitle'));
    myModal.modal('show');
}

function updateText() {
    language = $('.language option:selected').val();
    var i18n = $.i18n();
    i18n.locale = language;

    i18n.load('/Content/locals/en.json', 'en').done(function () { //Always load the english base version (some browser like FF dont reset dropdown menus)
        i18n.load('/Content/locals/' + i18n.locale + '.json', i18n.locale).done(function () {
            $('body').i18n();
            //Fullcalendar Custom Button i18n Text Injection becasue of bad button support in framework
            try {
                $(".fc-resetButton-button")[0].innerHTML = $.i18n('$mainapp_calendar_reset_button');
                $('.fc-resetButton-button').addClass("btn-crimson");
                console.log("Main UI loaded! (Lang: " + i18n.locale + ")");
            } catch (e) {
                console.log("Login Screen loaded! (Lang: " + i18n.locale + ")"); }
        });
    });
}

function cookieInit() {
    //Lang detection
    var lang = window.navigator.languages ? window.navigator.languages[0] : null;
    lang = lang || window.navigator.language || window.navigator.browserLanguage || window.navigator.userLanguage;
    if (lang.indexOf('-') !== -1)
        lang = lang.split('-')[0];

    if (lang.indexOf('_') !== -1)
        lang = lang.split('_')[0];

    //Cookie Language Check
    var cookieCheck = Cookies.get('onAirWebLang');
    if (typeof cookieCheck !== "undefined") {
        //Load last lang from cookie with jquery
        $("#langSelect").selectpicker('val', cookieCheck);
    } else {

        var langFound = false;
        //Search if browserlanguage is allowed (is in dropdown menu as option)
        $("#langSelect option").each(function (i) {
            if ($(this).val() === lang) { langFound = true; }
        });

        //Create default cookie with detected as default lang (english will be loaded if browser language is not found in dropdown menu)
        if (langFound) { Cookies.set('onAirWebLang', lang, { expires: 365 }); }
        else { Cookies.set('onAirWebLang', 'en', { expires: 365 }); }

        $("#langSelect").selectpicker('val', Cookies.get('onAirWebLang'));
    }

    //when cookie contains value that isn't there pick english as fallback
    if ($("#langSelect").val() === null) { 
        $("#langSelect").val("en");
    }
}

$(function () {
    console.log("Codename Yami - IRMA onAir Web");

    // Async Load & Render all i18n strings
    cookieInit();
    updateText();

    // Event listener stuff
    $('.language').on('change keyup', updateText);
    $("#langSelect").change(function () { Cookies.set('onAirWebLang', this.value, { expires: 365 }); });
    $("#loginFailedButton").click(function () { showLogin(); });
});
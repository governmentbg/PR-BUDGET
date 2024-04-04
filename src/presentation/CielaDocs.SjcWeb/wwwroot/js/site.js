const selectBoxMonthData = [{ id: 0, name: "Изберете месец" }, { id: 1, name: "Януари" }, { id: 2, name: "Февруари" }, { id: 3, name: "Март" }, { id: 4, name: "Април" }, { id: 5, name: "Май" }, { id: 6, name: "Юни" }, { id: 7, name: "Юли" }, { id: 8, name: "Август" }, { id: 9, name: "Септември" },
    { id: 10, name: "Октомври" },{id: 11,name: "Ноември"},{id: 12,name: "Декември"}];
const selectBoxYearData = [{ id: 0, name: "Изберете година" }, { id: 2022, name: "2022" }, { id: 2023, name: "2023" }, { id: 2024, name: "2024" }, { id: 2025, name: "2025" }, { id: 2026, name: "2026" }, { id: 2027, name: "2027" },
    { id: 2028, name: "2028" }, { id: 2029, name: "2029" }, { id: 2030, name: "2030" }, { id: 2031, name: "2031" }, { id: 2032, name: "2032" }];
const selectBoxQuarterData = [{ id: 0, name: "Изберете тримесечие" }, { id: 3, name: "Март" }, { id: 6, name: "Юни" }, { id: 9, name: "Септември" }, { id: 12, name: "Декември" }];

var loadingModal = $("#loadingModal");
var selectedCardId = 0;
var selectedEmplId = 0;
var actionInProgress = false;
var nextActionQueue = [];

function warn(s) {
    $.alert({
        boxWidth: '30%',
        useBootstrap: false,
        title: 'Внимание!',
        content: s
    });
}
function error(s) {
    $.alert({
        boxWidth: '30%',
        useBootstrap: false,
        title: 'Грешка!',
        content: s
    });
}
function showmsg(s) {
    $.alert({
        boxWidth: '30%',
        useBootstrap: false,
        title: 'Съобщение!',
        content: s
    });
}
function info(s) {
    $.alert({
        boxWidth: '30%',
        useBootstrap: false,
        title: 'Информация',
        content: s
    });
}
function confirmDlg(cnt, fn) {
    $.confirm({
        boxWidth: '30%',
        useBootstrap: false,
        title: 'Потвърждение',
        content: cnt,
        autoClose: 'cancelAction|8000',
        buttons: {
            confirmAction: {
                text: 'Добре',
                action: function () {
                    fn(true);
                }
            },
            cancelAction: {
                text: 'Затвори',
                action: function () {

                    return;
                }
            }
        }
    });
}
var redirectme = function (redirectUrl) {
    var form = $('<form action="' + redirectUrl + '" method="post"></form>');
    $('body').append(form);
    $(form).submit();
};
var redirectmeNew = function (redirectUrl) {
    var form = $('<form action="' + redirectUrl + '" method="post" target="_blank"></form>');
    $('body').append(form);
    $(form).submit();
};
var redirectme1 = function (redirectUrl, arg, value) {
    var form = $('<form action="' + redirectUrl + '" method="post">' +
        '<input type="hidden" name="' + arg + '" value="' + value + '"></input>' + '</form>');
    $('body').append(form);
    $(form).submit();
};
var redirectme1New = function (redirectUrl, arg, value) {
    var form = $('<form action="' + redirectUrl + '" method="post" target="_blank" >' +
        '<input type="hidden" name="' + arg + '" value="' + value + '"></input>' + '</form>');
    $('body').append(form);
    $(form).submit();
};
var redirectme2 = function (redirectUrl, arg, value, arg2, value2) {
    var form = $('<form action="' + redirectUrl + '" method="post">' +
        '<input type="hidden" name="' + arg + '" value="' + value + '"></input>' +
        '<input type="hidden" name="' + arg2 + '" value="' + value2 + '"></input>' +
        '</form>');
    $('body').append(form);
    $(form).submit();
};
var redirectme2New = function (redirectUrl, arg, value, arg2, value2) {
    var form = $('<form action="' + redirectUrl + '" method="post" target="_blank">' +
        '<input type="hidden" name="' + arg + '" value="' + value + '"></input>' +
        '<input type="hidden" name="' + arg2 + '" value="' + value2 + '"></input>' +
        '</form>');
    $('body').append(form);
    $(form).submit();
};
var redirectme3New = function (redirectUrl, arg, value, arg2, value2,arg3,value3) {
    var form = $('<form action="' + redirectUrl + '" method="post" target="_blank">' +
        '<input type="hidden" name="' + arg + '" value="' + value + '"></input>' +
        '<input type="hidden" name="' + arg2 + '" value="' + value2 + '"></input>' +
        '<input type="hidden" name="' + arg3 + '" value="' + value3 + '"></input>' +
        '</form>');
    $('body').append(form);
    $(form).submit();
};
var redirectme5New = function (redirectUrl, arg, value, arg2, value2, arg3, value3, arg4, value4, arg5, value5) {
    var form = $('<form action="' + redirectUrl + '" method="post" target="_blank">' +
        '<input type="hidden" name="' + arg + '" value="' + value + '"></input>' +
        '<input type="hidden" name="' + arg2 + '" value="' + value2 + '"></input>' +
        '<input type="hidden" name="' + arg3 + '" value="' + value3 + '"></input>' +
        '<input type="hidden" name="' + arg4 + '" value="' + value4 + '"></input>' +
        '<input type="hidden" name="' + arg5 + '" value="' + value5 + '"></input>' +
        '</form>');
    $('body').append(form);
    $(form).submit();
};
$('.modal-content').resizable({
    minHeight: 300,
    minWidth: 300
});
function showModal(modal) {
    if (actionInProgress) {
        nextActionQueue.push({
            id: modal.attr('id'),
            action: showModal
        });
        return;
    }

    actionInProgress = true;
    modal.on('shown.bs.modal', showCompleted);
    $('.modal-content').resizable({
        minHeight: 300,
        minWidth: 300
    });
    $('.modal-dialog').draggable();

    modal.on('show.bs.modal', function () {

        $(this).find('.modal-body').css({
            //'max-height': '100%'
            // 'max-height':'600px'
            'max-height': Math.max(0, ($(window).height() - 200))
        });
    });
    modal.modal("show");
    if (modal.parent().get(0).tagName != 'body')
        $('.modal-backdrop').insertAfter(modal);

    function showCompleted() {
        actionInProgress = false;
        modal.off('shown.bs.modal', showCompleted);
        if (nextActionQueue.length > 0) {
            var next = nextActionQueue.shift();
            next.action($("#" + next.id));
        }
    }

};


function hideModal(modal) {
    if (actionInProgress) {
        nextActionQueue.push({
            id: modal.attr('id'),
            action: hideModal
        });
        return;
    }

    actionInProgress = true;
    modal.on('hidden.bs.modal', hideCompleted);
    modal.modal("hide");

    function hideCompleted() {
        actionInProgress = false;
        modal.off('hidden.bs.modal', hideCompleted);
        if (nextActionQueue.length > 0) {
            var next = nextActionQueue.shift();
            next.action($("#" + next.id));
        }
    }
};
jQuery(function ($) {
    $.datepicker.regional['bg'] = {
        closeText: 'затвори',
        prevText: '&#x3c;назад',
        nextText: 'напред&#x3e;',
        nextBigText: '&#x3e;&#x3e;',
        currentText: 'днес',
        monthNames: ['Януари', 'Февруари', 'Март', 'Април', 'Май', 'Юни',
            'Юли', 'Август', 'Септември', 'Октомври', 'Ноември', 'Декември'],
        monthNamesShort: ['Яну', 'Фев', 'Мар', 'Апр', 'Май', 'Юни',
            'Юли', 'Авг', 'Сеп', 'Окт', 'Нов', 'Дек'],
        dayNames: ['Неделя', 'Понеделник', 'Вторник', 'Сряда', 'Четвъртък', 'Петък', 'Събота'],
        dayNamesShort: ['Нед', 'Пон', 'Вто', 'Сря', 'Чет', 'Пет', 'Съб'],
        dayNamesMin: ['Не', 'По', 'Вт', 'Ср', 'Че', 'Пе', 'Съ'],
        weekHeader: 'Wk',
        dateFormat: 'dd.mm.yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['bg']);
});
function daysBetweenBGDates(d1, d2) {
    var fromdate = d1.split('.');
    from_date = new Date(fromdate[2], fromdate[1], fromdate[0]);
    var todate = d2.split('.');
    to_date = new Date(todate[2], todate[1], todate[0]);
    var timeDiff = to_date.getTime() - from_date.getTime();
    return timeDiff / (1000 * 3600 * 24);

}

function showMsg(message) {
    $('#success-alert').html("<div class=\"alert alert-success\"><a href=\"#\" class=\"close\" data-dismiss=\"alert\">&times;</a>" + message + "</div>");
    $("#success-alert").fadeTo(4000, 500).slideUp(500, function () {
        $("#success-alert").slideUp(500);
    });
};
function showSuccess(message) {
    $('#success-alert').html("<div class=\"alert alert-success\"><a href=\"#\" class=\"close\" data-dismiss=\"alert\">&times;</a>" + message + "</div>");
    $("#success-alert").fadeTo(4000, 500).slideUp(500, function () {
        $("#success-alert").slideUp(500);
    });
};
function showError(message) {
    $('#success-alert').html("<div class=\"alert alert-danger\"><a href=\"#\" class=\"close\" data-dismiss=\"alert\">&times;</a>" + message + "</div>");
    $("#success-alert").fadeTo(4000, 500).slideUp(500, function () {
        $("#success-alert").slideUp(500);
    });
};
async function isEdeliveryEnabled(url,schoolId) {
    return await fetch(url + '?schoolId=' + schoolId )
        .then(response => {
            return response.json();
        });
}
async function isCardGuidExists(url, cardGuid) {
    return await fetch(url + '?cardGuid=' + cardGuid)
        .then(response => {
            return response.json();
        });
}
async function getEdeliveryData(url, schoolId) {
    return await fetch(url + '?schoolId=' + schoolId)
        .then(response => {
            return response.json();
        });
}
async function getCardRegistered(url, Id) {
    return await fetch(url + '?edeliveryId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getUnreadedMsg(url, Id) {
    return await fetch(url + '?emplId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getSignalsMsg(url,Id) {
    return await fetch(url + '?onrId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getEdeliveryUnreadedMsg(url, Id) {
    return await fetch(url + '?schoolId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getEdeliveryNewReceivedMsg(url) {
    return await fetch(url)
        .then(response => {
            return response.json();
        });
}
async function getEdeliveryNewReceivedMsg(url) {
    return await fetch(url)
        .then(response => {
            return response.json();
        });
}
async function getCardLocationEmpl(url, Id) {
    return await fetch(url + '?cardId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getCardDetailsByCardId(url, Id) {
    return await fetch(url + '?cardId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getExpiredCardAnswers(url, Id) {
    return await fetch(url + '?onrId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getExpiredCardAnswers(url, Id) {
    return await fetch(url + '?onrId=' + Id)
        .then(response => {
            return response.json();
        });
}
async function getDaysFromCardRoute(url, Id) {
    return await fetch(url + '?cardRouteId=' + Id)
        .then(response => {
            return response.json();
        });
}
function sendRequest(url, method = 'GET', data) {
    const d = $.Deferred();


    $.ajax(url, {
        method,
        data,
        cache: false,
        xhrFields: { withCredentials: true },
    }).done((result) => {
        d.resolve(method === 'GET' ? result.data : result);
    }).fail((xhr) => {
        d.reject(xhr.responseJSON ? xhr.responseJSON.Message : xhr.statusText);
    });

    return d.promise();
}
function isEmpty(str) {
    return (!str || str.length === 0);
}
function parseDMY(s) {
    if (isEmpty(s)) return new Date();
    let datepart = s.split(' ');
    let dt = datepart[0].split('.');
    let tm = datepart[1].split(':');
    return new Date(dt[2], dt[1] - 1, dt[0], tm[0], tm[1]);
}
function parseCyrilicDate(s) {
    if (isEmpty(s)) return new Date();
    let dt = s.split('.');
    return new Date(dt[2], dt[1] - 1, dt[0], '23','59','59');
}
function truncateString(str, length) {
    return str.length > length ? str.substring(0, length - 3) + '...' : str
}
function validateDigits(evt) {
    var theEvent = evt || window.event;


    // Handle key press
    var key = theEvent.keyCode || theEvent.which;
    key = String.fromCharCode(key);

    var regex = /[0-9]|\./;
    if (!regex.test(key)) {
        theEvent.returnValue = false;
        if (theEvent.preventDefault) theEvent.preventDefault();
    }
}
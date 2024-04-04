"use strict";

var DeloSpec = function () {
    var that = this,
        currentTask = {},
        currentNote = {},
        noteInfoForm = {},
        taskInfoForm = {},
        currentCard = {},
        cardInfoForm = {},
        selectedCardId=0,
        propertyIndexes = {
            "priority": {
                "Критичен": 4,
                "Висок": 3,
                "Среден": 2,
                "Нисък": 1
            },
            "status": {
                "Приключена": 1,
                "Не е стартирана": 2,
                "В процес на изпълнение": 3,
                "Необходимост от помощ": 4,
                "Отложена": 5
            }
        },
        priorities = [
            { id: 1, value: "Нисък" },
            { id: 2, value: "Среден" },
            { id: 3, value: "Висок" },
            { id: 4, value: "Критичен" }
        ],
        statuses = ["Приключена", "Не е стартирана", "В процес на изпълнение", "Необходимост от помощ", "Отложена"],
        sections = [
            { text: "Оценки" },
            { text: "Задачи" }
        ];

    var editNoteInited = $.Deferred(),
        editTaskInited = $.Deferred(),
        editCardInited = $.Deferred();

    EduDl.DBCustomFilterFields = [
       
    ];

    that.cardGrid = {};
    that.tabs = {};

    EduDl.CardGrid = that.cardGrid;

    var cardStore = EduDl.stores.delospecstore,
        cardDataSource = {
            store: cardStore,
        };


    var gridOptions = {
        dataSource: {},
        allowColumnReordering: true,
        allowColumnResizing: true,
        filterRow: { visible: true },
       paging: {
            pageSize: 10
        },
        pager: {
            visible: true,
            showNavigationButtons: true,
            allowedPageSizes: [10, 25, 50, 100],
            showPageSizeSelector: true,
            showInfo: true,
            showNavigationButtons: true,
        },
        selection: {
            mode: "single"
        },
        rowAlternationEnabled: true,
        hoverStateEnabled: true,
        columnAutoWidth: true,
        wordWrapEnabled: false, 
        columns: [
            , {
                dataField: "LockedById",
                caption: "Приет",
                width: "4%",
                cellTemplate: function (element, info) {
                    if (info.text == '0') {
                        element.append("<div></div>");
                    }
                    else if (info.text != '0') {
                        element.append("<div><img alt='.' src='./images/lock.png'></img></div>");
                    }
                   
                },
            },
            {
                dataField: "DocNum",
                caption: "Рег.№",
                width:"12%",
            },
            {
                dataField: "DocDate",
                caption: "От дата",
                dataType: "date",
                width: "10%",
            }
            ,
            {
                dataField: "DocTypeDtos.Name",
                caption: "Тип",
                width: "10%",
            }
            , {
                dataField: "DocTypeId",
                caption: "Тип",
                width: "4%",
                cellTemplate: function (element, info) {
                    if (info.text == '0') {
                        element.append("<div><img alt='.' src='./images/empty-flag.png'></img></div>");
                    }
                    else if (info.text == '1') {
                        element.append("<div><img alt='.' src='./images/green-flag.png'></img></div>");
                    }
                    else if (info.text == '2') {
                        element.append("<div><img alt='.' src='./images/blue-flag.png'></img></div>");
                    }
                    else if (info.text == '3') {
                        element.append("<div><img alt='.' src='./images/yellow-flag.png'></img></div>");
                    }
                    else if (info.text == '4') {
                        element.append("<div><img alt='.' src='./images/red-flag.png'></img></div>");
                    }
                    else if (info.text == '5') {
                        element.append("<div><img alt='.' src='./images/orange-flag.png'></img></div>");
                    }
                    else if (info.text == '6') {
                        element.append("<div><img alt='.' src='./images/darkorange-flag.png'></img></div>");
                    }
                },
            }
            , {
                dataField: "DocKindOfDtos.Name",
                caption: "Вид",
                width: "15%",

            }
            , {
                dataField: "CorrName",
                width: "20%",
                caption: "От/До"
            }, {
                dataField: "Subject",
                caption: "Относно",
                width: "45%",
                customizeText: function (cellInfo) {
                    return truncateString(cellInfo.value,100);
                }
            },
           
           {
                type: 'buttons',
                width: 96,
               buttons: [{
                   hint: 'Редакция',
                   icon: 'edit',

                   onClick(e) {
                       editCardNav(e.row.data.Id)
                       e.component.refresh(true);
                       e.event.preventDefault();
                   },
               },
                    {
                        hint: 'Информация',
                        icon: 'folder',

                        onClick(e) {
                            showCard(e.row.data.Id)
                            e.component.refresh(true);
                            e.event.preventDefault();
                        },
                    }
                ],
            }
        ],
        
        masterDetail: {
            enabled: true,
            template(container, options) {
                const currentCardRowData = options.data;

    $('<div>')
        .addClass('master-detail-caption')
        .text(`${currentCardRowData.DocNum} ,№ ${currentCardRowData.CorrName} Свързани преписки:`)
        .appendTo(container);

    $('<div>')
        .dxDataGrid({
            columnAutoWidth: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            showBorders: true,
          
            columns: [
                { dataField: 'DocNum', caption: 'Рег.№', width: "10%" },
                { dataField: 'DocDate', caption: 'От дата', dataType: 'date', format: 'dd.MM.yyyy', width: "10%" },
                { dataField: 'DocTypeDtos.Name', caption: 'Тип', width: "10%" },
                { dataField: 'DocKindOfDtos.Name', caption: 'Вид', width: "15%" },
                { dataField: 'CardStatusDtos.Name', caption: 'Статус', width: "15%" },
                { dataField: 'Subject', caption: 'Относно', width: "30%" },
                {
                    type: 'buttons',
                    width: 64,
                    buttons: [{
                        hint: 'Информация',
                        icon: 'info',

                        onClick(e) {
                            showCard(e.row.data.Id)
                            e.component.refresh(true);
                            e.event.preventDefault();
                        },
                    },
                    ],
                },
            ],
            dataSource: new DevExpress.data.DataSource({
                store: new DevExpress.data.CustomStore({
                    key: 'Id',
                    load() {
                        return sendRequest(EduDl.baseUrl + "DeloSpecDx/GetCardsByMasterId", 'Get', { onrId: EduDl.OnrId, cardId: options.key });
                    }
                }),
            }),
            paging: {
                pageSize: 10,
            },
            pager: {
                visible: true,
                allowedPageSizes: [5, 10, 'Всички'],
                showPageSizeSelector: true,
                showInfo: false,
                showNavigationButtons: true,
            },
        }).appendTo(container);
},
        },
        onContextMenuPreparing: function (e) {
            if (e.row.rowType === "data") {
                e.items = [{
                    text: "Назначаване на проверка",
                    onItemClick: function () {
                        addCardAction('1');
                    }
                },
                    {
                        text: "Резултат от проверка",
                        onItemClick: function () {
                            addCardAction('2');
                        }
                    }
                    , {
                    text: "Права върху преписка",
                    onItemClick: function () {
                        addPermission(e.row.data.Id);
                    }
                    },
                    {
                        text: "Архивиране",
                        onItemClick: function () {
                            addCardAction('3');
                        }
                    }];
            }
        },
        onRowRemoved: function (e) {
            e.component.selectRowsByIndexes(0);
        },
        onSelectionChanged: function (items) {
            if (items.selectedRowsData[0]) {
                selectedCardId = items.selectedRowsData[0].Id;
            }
        },
        focusedRowEnabled: true,
        onFocusedRowChanged(e) {
            if (e) {
                 getExtraDataItem(e.row);
            }
        }
    };

 
    async function getExtraDataItem(row) {

        $('#card-info').empty();
        $.ajax({
            url: EduDl.baseUrl + "DeloSpecDx/GetCardDetails", data: { cardId: row.data.Id }, success: function (result) {
 
                $('#card-info').html(result);
            }
        });
        return false;
    }
    function addCardAction(actionId) {
               
        if (selectedCardId == 0) {
            DevExpress.ui.notify('Не сте селектирали преписка! Кликнете върху реда от таблицата с желаната преписка');
            return;
        }
        switch (actionId) {
            case "1": 
                addCardLinkedNav(selectedCardId);
                break;
            case "2": {
                let checkCardUrl = EduDl.baseUrl + "CardAction/CheckCardEnabledForReply";

                $.ajax({
                    url: checkCardUrl,
                    data: { cardId: selectedCardId },
                    success: function (data) {
                        if (data.success == false) {
                            warn(data.msg);
                            return;
                        }
                        else {
                            addCardReplyNav(selectedCardId);
                        }
                    }
                });
            }
                   
                   break;
            case "3": {
                let urlcompl = EduDl.baseUrl + "CardAction/GetCardForCompletion";
                cardActionContent.empty();
                cardActionContent.html('<p>Зареждам данни…</p>')
                $.ajax({
                    type: 'POST',
                    url: urlcompl,
                    data: { cardId: selectedCardId },
                    success: function (data) {
                        cardActionContent.html(data);
                    }
                });
                cardActionModal.modal('show');
            }
                break;
            case "4": {
                let urlcompl = EduDl.baseUrl + "CardAction/GetCardForArchiving";
                cardActionContent.empty();
                cardActionContent.html('<p>Зареждам данни…</p>')
                $.ajax({
                    type: 'POST',
                    url: urlcompl,
                    data: { cardId: selectedCardId },
                    success: function (data) {
                        cardActionContent.html(data);
                    }
                });
                cardActionModal.modal('show');
            }
                break;
        }
        
    }
    function addCardReplyNav(cardMasterId) {
        let cardreplyurl = EduDl.baseUrl + "DeloSpecDx/GetCardNavMasterPartial";
        commonContent.empty();
        commonContent.html('<p>Зареждам данни…</p>')
        $.ajax({
            type: 'Get',
            url: cardreplyurl,
            data: { cardMasterId: cardMasterId, actionType: 'reply' },
            success: function (data) {
                commonContent.html(data);
            }
        });
        commonModal.modal('show');
    }
    function addCardLinkedNav(cardMasterId) {
        let cardlinkurl = EduDl.baseUrl + "DeloSpecDx/GetCardNavMasterPartial";
        commonContent.empty();
        commonContent.html('<p>Зареждам данни…</p>')
        $.ajax({
            type: 'Get',
            url: cardlinkurl,
            data: { cardMasterId: cardMasterId, actionType: 'linked' },
            success: function (data) {
                commonContent.html(data);
            }
        });
        commonModal.modal('show');
    }
    function addCardMovement(movementId) {
        if (selectedCardId == 0) {
            warn('Не сте селектирали преписка! Кликнете върху реда от таблицата с желаната преписка');
            return;
        }
        switch (movementId) {
            case "0": {

                let empldirectUrl = EduDl.baseUrl + "EmplDirect/AddMessagePartialView";
                commonContent.empty();
                commonContent.html('<p>Зареждам данни…</p>')
                $.ajax({
                    type: 'Get',
                    url: empldirectUrl,
                    success: function (data) {
                        commonContent.html(data);
                    }
                });
                commonModal.modal('show');
            }
                break;
            case "1": {
               
                let empldirectUrl = EduDl.baseUrl + "EmplDirect/DocToEmplDxPartialView";
                    commonContent.empty();
                    commonContent.html('<p>Зареждам данни…</p>')
                    $.ajax({
                        type: 'Get',
                        url: empldirectUrl,
                        data: { cardId: selectedCardId },
                        success: function (data) {
                            commonContent.html(data);
                        }
                    });
                 commonModal.modal('show');
            }
                break;
            case "2":
                let cardmovesUrl = EduDl.baseUrl + "EmplDirect/LoadCardMovementGrid/?cardId=" + selectedCardId;
                window.open(cardmovesUrl, '_blank');
                break;
        }
    }
    function addCardNav(docTypeId) {
        let cardurl = EduDl.baseUrl + "DeloSpecDx/GetCardNavPartial";
        cardContent.empty();
        cardContent.html('<p>Зареждам данни…</p>')
        $.ajax({
            type: 'Get',
            url: cardurl,
            data: { onrId: EduDl.OnrId, docTypeId: docTypeId },
            success: function (data) {
                cardContent.html(data);
            }
        });
 
        cardModal.modal('show');
    }
    function editCardNav(id) {
        let cardurl = EduDl.baseUrl + "DeloSpecDx/GetCardNavEditPartial";
         commonContent.empty();
        commonContent.html('<p>Зареждам данни…</p>')
        $.ajax({
            type: 'Get',
            url: cardurl,
            data: { cardId: id },
            success: function (data) {
                commonContent.html(data);
            }
        });

        commonModal.modal('show');
    }
   

    function editCard(e, options) {
        currentCard = options.data ? options.data : options;
        that.editCard.option("visible", true);
    }
    const menuData = [ {
        id: '2',
        name: 'Действие',
        items: [{
            id: '2_1',
            name: 'Назначаване на проверка',

        }, {
            id: '2_2',
            name: 'Резултат от проверка',
        }, {
            id: '2_3',
            name: 'Архивиране',

            }, {
                id: '2_9',
                name: 'Права върху преписка',

            }],
    }, {
        id: '3',
        name: 'Движение',
        items: [{
            id: '3_0',
            name: 'Изпрати съобщение...',

        }, {
                id: '3_1',
                name: 'Изпрати преписка...',

            }, {
                id: '3_2',
                name: 'Движения на документа',
            }],
        }, {
            id: '6',
        name: 'Помощни',
        items: [{
            id: '6_1',
            name: 'Експорт (Excel)',
        }
            ],

        }, {
            id: '7',
            name: 'Филтър',
            icon: './images/filter.png',

        }, {
            id: '4',
        name: 'Обнови',
        icon: './images/refresh.png',
           
        }, {
            id: '5',
        name: 'Затвори',
            icon:'./images/close.png',

        }];


    that.init = function () {
        that.cardGrid = EduDl.createGrid(gridOptions, cardDataSource);

        //-----------test--------------------
        const dxMenu = $('#menu').dxMenu({
            dataSource: menuData,
            hideSubmenuOnMouseLeave: false,
            displayExpr: 'name',
            onItemClick(data) {
                const item = data.itemData;
                if (item.id) {
                    switch (item.id) {
                        case '1_1':  addCardNav(1);
                            break;
                        case '1_2': addCardNav(2);
                            break;
                        case '1_3': addCardNav(4);
                            break;

                        case '2_1': addCardAction('1');
                            break;
                        case '2_2': addCardAction('2');
                            break;
                        case '2_3': addCardAction('3');
                            break;
                        case '2_4': addCardAction('4');
                            break;
                        case '2_9':
                            addPermission(selectedCardId);
                            break;
                        case '3_0': addCardMovement('0');
                            break;
                        case '3_1': addCardMovement('1');
                            break;
                        case '3_2': addCardMovement('2');
                            break;
                        case '4':
                            that.cardGrid.refresh();
                            $('#card-info').empty();
                            break;
                        case '5':
                            window.location.href = EduDl.baseUrl;
                            break;
                        case '6_1':
                            var workbook = new ExcelJS.Workbook();
                            var worksheet = workbook.addWorksheet("CardsSpec");

                            DevExpress.excelExporter.exportDataGrid({
                                component: $("#grid").dxDataGrid("instance"),
                                worksheet: worksheet
                            }).then(function () {
                                workbook.xlsx.writeBuffer().then(function (buffer) {
                                    saveAs(new Blob([buffer], { type: "application/octet-stream" }), "CardsSpec.xlsx");
                                });
                            });
                            break;
                       
                        
                        case '6_3': download(EduDl.baseUrl + "twain/Saraff.Twain.Service_x64.msi");
                            break;
                        case '6_4': download(EduDl.baseUrl + "twain/Saraff.Twain.Service_x86.msi")
                            break;
                        case '7': addCardFilter(EduDl.OnrId);
                            break;
                    }
                }
            },
        }).dxMenu('instance');

        const showSubmenuModes = [{
            name: 'onHover',
            delay: { show: 0, hide: 500 },
        }, {
            name: 'onClick',
            delay: { show: 0, hide: 300 },
        }];

       
    };
   
};
function download(link) {
    var element = document.createElement('a');
    element.setAttribute('href', link);

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
}

//EduDl.filterField = "DocTypeDtos.Id";
//EduDl.customFilters.init("0");
const downloads = ['Изтегли приложение за онлайн сканиране x64', 'Изтегли приложение за онлайн сканиране x86', 'Package Managers'];
$(function () {
    var delospec = new DeloSpec();
    delospec.init();
  
});


"use strict";

var MessagesOut = function () {
    var that = this,
        currentMsg = {},
        currentCard = {},
        cardInfoForm = {},
        selectedCardId = 0,
        selectedMsgId = 0,
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

 

   

    that.msgGrid = {};
    that.tabs = {};

    EduDl.msgGrid = that.msgGrid;

    var msgStore = EduDl.stores.mymessagesOutStore,
        msgDataSource = {
            store: msgStore,
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
        columns: [

            
            {
                dataField: "EmplMessageDtos.ReceivedDate",
                caption: "От дата",
                dataType: "date",
                format: "dd.MM.yyyy HH:mm:ss",
                width: "10%",
            }
            , {
                dataField: "DirectTypeDtos.Name",
                caption: "Тип",
                width: "10%",
                customizeText: function (cellInfo) {
                    return EduDl.formatDocType(cellInfo.valueText);
                },
                visible: true
            }, {
                dataField: "RecipientEmplDtos.Name",
                caption: "До",
                width: "15%",

            },
            {
                dataField: "Subject",
                caption: "Заглавие",
                width: "65%",
            },
            {
                type: 'buttons',
                width: 96,
                buttons: [
                    {
                        hint: 'Преглед',
                        icon: 'info',

                        onClick(e) {
                            showMessage(e.row.data.Id)
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
                const currentMsgRowData = options.data;

                $('<div>')
                    .addClass('master-detail-caption')
                    .text(`${currentMsgRowData.SenderEmplDtos.EmplName} , ${currentMsgRowData.Subject} Свързани:`)
                    .appendTo(container);

                $('<div>')
                    .dxDataGrid({
                        columnAutoWidth: true,
                        allowColumnReordering: true,
                        allowColumnResizing: true,
                        showBorders: true,
                        columns: [
                            {
                                dataField: "EmplMessageDtos.ReceivedDate",
                                caption: "От дата",
                                dataType: "date",
                                format: "dd.MM.yyyy HH:mm:ss",
                                width: "10%",
                            }
                            , {
                                dataField: "DirectTypeDtos.Name",
                                caption: "Тип",
                                width: "10%",
                                customizeText: function (cellInfo) {
                                    return EduDl.formatDocType(cellInfo.valueText);
                                },
                                visible: true
                            }, {
                                dataField: "SenderEmplDtos.Name",
                                caption: "От",
                                width: "15%",

                            }, {
                                dataField: "RecipientEmplDtos.Name",
                                caption: "До",
                                width: "15%",

                            },
                            {
                                dataField: "Subject",
                                caption: "Заглавие",
                                width: "65%",
                            },
                            {
                                type: 'buttons',
                                width: 96,
                                buttons: [
                                    {
                                        hint: 'Преглед',
                                        icon: 'info',

                                        onClick(e) {
                                            showMessage(e.row.data.Id)
                                            e.component.refresh(true);
                                            e.event.preventDefault();
                                        },
                                    }
                                ],
                            }
                        ],
                        dataSource: new DevExpress.data.DataSource({
                            store: new DevExpress.data.CustomStore({
                                key: 'Id',
                                load() {
                                    return sendRequest(EduDl.baseUrl + "MyMessagesDx/GetChildMessagesById", 'Get', { onrId: EduDl.OnrId, msgId: options.key });
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
        },
       
    };
    async function getExtraDataItem(row) {

        $('#card-info').empty();
        $.ajax({
            url: EduDl.baseUrl + "MyMessagesDx/GetMessageDetails", data: { msgId: row.data.Id }, success: function (result) {
 
                $('#card-info').html(result);
            }
        });
        return false;
    }
    const menuData = [{
        id: '1',
        name: 'Ново съобщение',
        icon: './images/add.png',
    }, {
        id: '6_1',
        name: 'Експорт',
        icon: './images/exporter.svg',

    }, {
        id: '4',
        name: 'Обнови',
        icon: './images/refresh.png',

    }, {
        id: '5',
        name: 'Затвори',
        icon: './images/close.png',

    }];

    that.init = function () {
        that.msgGrid = EduDl.createGrid(gridOptions, msgDataSource);

        const dxMenu = $('#menu').dxMenu({
            dataSource: menuData,
            hideSubmenuOnMouseLeave: false,
            displayExpr: 'name',
            onItemClick(data) {
                const item = data.itemData;
                if (item.id) {
                    switch (item.id) {
                        case '1': addNewMessage();
                            break;

                        case '4':
                            that.msgGrid.refresh();
                            $('#card-info').empty();
                            break;
                        case '5':
                            window.location.href = EduDl.baseUrl;
                            break;
                        case '6_1':
                            var workbook = new ExcelJS.Workbook();
                            var worksheet = workbook.addWorksheet("MessagesOut");

                            DevExpress.excelExporter.exportDataGrid({
                                component: $("#grid").dxDataGrid("instance"),
                                worksheet: worksheet
                            }).then(function () {
                                workbook.xlsx.writeBuffer().then(function (buffer) {
                                    saveAs(new Blob([buffer], { type: "application/octet-stream" }), "MessagesOut.xlsx");
                                });
                            });
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


//EduDl.filterField = "DocTypeDtos.Id";
//EduDl.customFilters.init("0");

$(function () {
    const readed = ['Всички', 'Непрочетени', 'Прочетени'];
    const directtypes = ['Всички', 'Съобщение', 'Задача', 'За резолюция', 'За съгласуване', 'За одобрение', 'За корекция', 'За подпис', 'За Деловодство', 'За последващи действия', 'За сведение', 'За становище', 'Отговор', 'Отговор на съгл. процедура'];

    var messagesOut = new MessagesOut();
    messagesOut.init();
    $('#selectReaded').dxSelectBox({
        dataSource: readed,
        value: readed[0],
        onValueChanged(data) {
            if (data.value === 'Всички') { $("#grid").dxDataGrid("instance").clearFilter(); } else { $("#grid").dxDataGrid("instance").filter(['IsReaded', '=', (data.value == 'Прочетени') ? true : false]); }
        },
    });
    $('#selectDirectType').dxSelectBox({
        dataSource: directtypes,
        value: directtypes[0],
        onValueChanged(data) {
            if (data.value === 'Всички') { $("#grid").dxDataGrid("instance").clearFilter(); } else { $("#grid").dxDataGrid("instance").filter(['DirectTypeId', '=', directtypes.indexOf(data.value)]); }
        },
    });
});


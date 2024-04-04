"use strict";

var Archive = function () {
    var that = this,
        selectedCardId = 0;
   
       
    that.cardGrid = {};
    that.tabs = {};

    EduDl.CardGrid = that.cardGrid;

    var cardStore = EduDl.stores.archivestore,
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
        columns: [

            {
                dataField: "DocNum",
                caption: "Рег.№",
                width: "8%",
            },
            {
                dataField: "DocDate",
                caption: "От дата",
                dataType: "date",
                width: "8%",
            }
            , {
                dataField: "DocTypeDtos.Name",
                caption: "Тип",
                width: "5%",
                customizeText: function (cellInfo) {
                    return EduDl.formatDocType(cellInfo.valueText);
                },
                visible: true
            }, {
                dataField: "DocKindOfDtos.Name",
                caption: "Вид",
                width: "10%",

            }
            , {
                dataField: "CorrName",
                width: "15%",
                caption: "Кореспондент"
            }, {
                dataField: "Subject",
                caption: "Относно",
                width: "65%",
            },
            {
                dataField: "EndDate",
                caption: "Крайна дата",
                dataType: "date",
                width: "8%",
            },
            {
                type: 'buttons',
                width: 96,
                buttons: [
                {
                    hint: 'Информация',
                    icon: 'info',

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
                                    return sendRequest(EduDl.baseUrl + "ArchiveDx/GetCardsByMasterId", 'Get', { onrId: EduDl.OnrId, cardId: options.key });
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
            url: EduDl.baseUrl + "DeloDx/GetCardDetails", data: { cardId: row.data.Id }, success: function (result) {

                $('#card-info').html(result);
            }
        });
        return false;
    }
    function addCardAction(actionId) {

        if (selectedCardId == 0) {
            warn('Не сте селектирали преписка! Кликнете върху реда от таблицата с желаната преписка');
            return;
        }
        switch (actionId) {
           
            case "1": {
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
                showModal(cardActionModal);
            }
                break;
        }

    }
    const menuData = [{
        id: '1',
        name: 'Отмени архивирането',
    }, {
        id: '6',
        name: 'Помощни',
        items: [{
            id: '6_1',
            name: 'Експорт (Excel)',
        }],

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
        that.cardGrid = EduDl.createGrid(gridOptions, cardDataSource);
        const dxMenu = $('#menu').dxMenu({
            dataSource: menuData,
            hideSubmenuOnMouseLeave: false,
            displayExpr: 'name',
            onItemClick(data) {
                const item = data.itemData;
                if (item.id) {
                    switch (item.id) {
                        case '1': addCardAction('1');
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
                            var worksheet = workbook.addWorksheet("Archived");

                            DevExpress.excelExporter.exportDataGrid({
                                component: $("#grid").dxDataGrid("instance"),
                                worksheet: worksheet
                            }).then(function () {
                                workbook.xlsx.writeBuffer().then(function (buffer) {
                                    saveAs(new Blob([buffer], { type: "application/octet-stream" }), "Archived.xlsx");
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


$(function () {
    var archive = new Archive();
    archive.init();
});


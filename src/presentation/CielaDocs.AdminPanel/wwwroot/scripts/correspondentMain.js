"use strict";

var CorrespondentMain = function () {
    var that = this,
        selectedCorrId = 0;


    that.cardGrid = {};
    that.tabs = {};

    Ciela.CardGrid = that.cardGrid;

    var corrStore = new DevExpress.data.CustomStore({
        key: 'Id',
        load() {
            return sendRequest(EduDl.baseUrl + "CorrespondentDx/GetCorrs", 'Get', { onrId: EduDl.OnrId });
        }
    });


    var gridOptions = {
        dataSource: corrStore, 
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
            { dataField: 'Name', caption: 'Кореспондент', width: "20%" },
            { dataField: 'Identifier', caption: 'Идентификатор', width: "10%" },
            { dataField: 'CountryDtos.Name', caption: 'Държава', width: "15%" },
            { dataField: 'TownDtos.Name', caption: 'Населено място', width: "15%" },
            { dataField: 'Address', caption: 'Адрес', width: "25%" },
            { dataField: 'Phone', caption: 'Тел.', width: "15%" },
            { dataField: 'EmailAddress', caption: 'Имейл', width: "15%" },
            {
                type: 'buttons',
                width: 110,
                buttons: [{
                    hint: 'Редакция',
                    icon: 'edit',

                    onClick(e) {
                        editCorr(e.row.data.Id)
                        e.component.refresh(true);
                        e.event.preventDefault();
                    },
                },
                {
                    hint: 'Премахни',
                    icon: 'trash',

                    onClick(e) {
                        deleteCorr(e.row.data.Id);
                        e.component.refresh(true);
                        e.event.preventDefault();
                    },
                }],
            },
        ],
        masterDetail: {
            enabled: true,
            template(container, options) {
                const currentCorrData = options.data;

                $('<div>')
                    .addClass('master-detail-caption')
                    .text(`${currentCorrData.Name} ,№ ${currentCorrData.Identifier} Преписки:`)
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
                                    return sendRequest(Ciela.baseUrl + "CorrespondentDx/GetCardsByCorrId", 'Get', { corrId: options.key });
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
        onSelectionChanged: function (items) {
            if (items.selectedRowsData[0]) {
                selectedCorrId = items.selectedRowsData[0].Id;
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
        //$.ajax({
        //    url: Ciela.baseUrl + "DeloDx/GetMessageRegistered", data: { msgId: row.data.Id }, success: function (result) {
        //        $('#card-info').html(result);
        //    }
        //});
        return false;
    }
    function addNewCorr() {
        let corr_url = Ciela.baseUrl + "CorrespondentDx/AddCorr";
        corrNewContent.empty();
        corrNewContent.html('<p>Зареждам данни…</p>')
        $.ajax({
            type: 'Get',
            url: corr_url,
            data: { onrId: Ciela.OnrId },
            success: function (data) {
                corrNewContent.html(data);
            }
        });
        showModal(corrModal);

    }

    function editCorr(id) {
        let corr_url = Ciela.baseUrl + "CorrespondentDx/EditCorr";
        corrNewContent.empty();
        corrNewContent.html('<p>Зареждам данни…</p>')
        $.ajax({
            type: 'Get',
            url: corr_url,
            data: { corrId: id },
            success: function (data) {
                corrNewContent.html(data);
            }
        });
        showModal(corrModal);
    }
    function deleteCorr(id) {
        if (confirmDlg('Потвърдете изтриването на записа', function (ret) {
            if (ret) {
                let delurl = Ciela.baseUrl + "CorrespondentDx/Delete";
                $.ajax({
                    url: delurl, data: { onrId: Ciela.OnrId, id: id }, method: 'POST'
                })
                    .done(function (data) {
                        if (data.success == true) {
                            dataGrid.refresh();
                            DevExpress.ui.notify("Записът бе изтрит");
                        }
                        else {
                            warn(data.msg)
                        }
                    })
                    .fail(function () {
                        DevExpress.ui.notify('Грешка при изтриване на записа.');
                    });
            }
        }));
    }    
    const menuData = [{
        id: '1',
        name: 'Добави',
        icon: './images/add.png',
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
        that.cardGrid = Ciela.createGrid(gridOptions, cardDataSource);
        const dxMenu = $('#menu').dxMenu({
            dataSource: menuData,
            hideSubmenuOnMouseLeave: false,
            displayExpr: 'name',
            onItemClick(data) {
                const item = data.itemData;
                if (item.id) {
                         switch (item.id) {
                        case '1': addNewCorr();
                            break;
                      
                        case '4':
                            that.cardGrid.refresh();
                            $('#card-info').empty();
                            break;
                        case '5':
                            window.location.href = Ciela.baseUrl;
                            break;
                        case '6_1':
                            var workbook = new ExcelJS.Workbook();
                            var worksheet = workbook.addWorksheet("Correspondents");

                            DevExpress.excelExporter.exportDataGrid({
                                component: $("#grid").dxDataGrid("instance"),
                                worksheet: worksheet
                            }).then(function () {
                                workbook.xlsx.writeBuffer().then(function (buffer) {
                                    saveAs(new Blob([buffer], { type: "application/octet-stream" }), "Correspondents.xlsx");
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
    var correspondentMain = new CorrespondentMain();
    correspondentMain.init();
});


"use strict";

window.EduDl = window.EduDl || {};
var CUSTOM_FILTER_KEY = "dx-custom-filter-data";
function toIsoString(date) {
    var tzo = -date.getTimezoneOffset(),
        dif = tzo >= 0 ? '+' : '-',
        pad = function (num) {
            return (num < 10 ? '0' : '') + num;
        };

    return date.getFullYear() +
        '-' + pad(date.getMonth() + 1) +
        '-' + pad(date.getDate()) +
        'T' + pad(date.getHours()) +
        ':' + pad(date.getMinutes()) +
        ':' + pad(date.getSeconds()) +
        dif + pad(Math.floor(Math.abs(tzo) / 60)) +
        ':' + pad(Math.abs(tzo) % 60);
}
function treatAsUTC(date) {
    var result = new Date(date);
    result.setMinutes(result.getMinutes() - result.getTimezoneOffset());
    return result;
}
function daysBetween(startDate, endDate) {
    var millisecondsPerDay = 24 * 60 * 60 * 1000;
    return parseInt((treatAsUTC(endDate) - treatAsUTC(startDate)) / millisecondsPerDay);
}

EduDl.stores = {
    delostore:  new DevExpress.data.CustomStore({
        key: 'Id',
        load() {
            var filterkey = window.localStorage.key('cardfilter');
            if (!filterkey) {
                return sendRequest(EduDl.baseUrl + "DeloDx/GetCardsByOnrId", 'Get', { onrId: EduDl.OnrId });
            }
            else {
                return sendRequest(EduDl.baseUrl + "DeloDx/GetCardsByOnrIdFilter", 'Get', { onrId: EduDl.OnrId, filterdata: window.localStorage.getItem('cardfilter') });
            }
        }
    }),
      delospecstore: new DevExpress.data.CustomStore({
          key: 'Id',
          load() {
              var filterkey = window.localStorage.key('cardfilter');
              if (!filterkey) {
                  return sendRequest(EduDl.baseUrl + "DeloSpecDx/GetCardsByOnrId", 'Get', { onrId: EduDl.OnrId });
              }
              else {
                  return sendRequest(EduDl.baseUrl + "DeloSpecDx/GetCardsByOnrIdFilter", 'Get', { onrId: EduDl.OnrId, filterdata: window.localStorage.getItem('cardfilter') });
              }
          }
      })
    ,
    edeliveryNewStore: new DevExpress.data.CustomStore({
        key: 'Id',
        load() {
            return sendRequest(EduDl.baseUrl + "EdeliveryDx/GetIncomingNew", 'Get', { onrId: EduDl.OnrId });
        }
    }), 
    edeliveryIncomingStore: new DevExpress.data.CustomStore({
        key: 'Id',
        load() {
            return sendRequest(EduDl.baseUrl + "EdeliveryIncomingDx/GetIncoming", 'Get', { onrId: EduDl.OnrId });
        }
    }),
    edeliveryOutgoingStore: new DevExpress.data.CustomStore({
        key: 'Id',
        load() {
            return sendRequest(EduDl.baseUrl + "EdeliveryOutgoingDx/GetOutgoing", 'Get', { onrId: EduDl.OnrId });
        }
    }),
    archivestore: new DevExpress.data.CustomStore({
        key: 'Id',
        load() {
            return sendRequest(EduDl.baseUrl + "ArchiveDx/GetCardsArchivedByOnrId", 'Get', { onrId: EduDl.OnrId });
        }
    }),
     corrStore:  new DevExpress.data.CustomStore({
        key: 'Id',
        load() {
            return sendRequest(EduDl.baseUrl +"CorrespondentDx/GetCorrs", 'Get', { onrId: EduDl.OnrId });
        }
    }),
  
};

EduDl.customFilters = {
    editingFilterName: "",
    currentFilterInfo: {},
    getFilterInfo: function() {
        if (this.currentFilterInfo)
            return this.currentFilterInfo[this.viewName];
        else
            return [];
    },

    getFilterInfoByName: function(filterName) {
        if (this.currentFilterInfo && this.currentFilterInfo[this.viewName].length !== 0) {
            var currentFilterData = this.currentFilterInfo[this.viewName][this.getIndexByName(filterName)];
            return (currentFilterData && currentFilterData.args) || [];
        }
    },
    menuSelector: "#custom-filters > div ul",
    viewName: "",
    applyCustomFilter: function(filterData) {
        this.editingFilterName = "";
        EduDl.filterGrid(filterData.filterExpression || filterData.args, "custom");
    },
    getIndexByName: function(filterName) {
        var result = -1;
        $.each(this.currentFilterInfo[this.viewName], function(index, item) {
            if (item.name == filterName) {
                result = index;
                return false;
            }
        });

        return result;
    },
    isFilterEmpty: function(filterData) {
        return (!filterData || !filterData.args || (filterData.args.length === 0) || (filterData.args.length == 1 && (!filterData.args[0].length || !filterData.args[0][0].length)));
    },
    saveViewFilter: function(filterData, saveToStore) {
        if (!this.currentFilterInfo)
            this.currentFilterInfo = {};

        if (!this.currentFilterInfo[this.viewName])
            this.currentFilterInfo[this.viewName] = [];

        var sameNameIndex = this.getIndexByName(filterData.name);

        filterData.saveToStore = saveToStore;
        if (sameNameIndex == -1) {
            this.currentFilterInfo[this.viewName].push(filterData);
        } else {
            this.currentFilterInfo[this.viewName][sameNameIndex] = filterData;
        }

        this.saveFiltersToStore();

        this.applyCustomFilter(filterData);
    },
    saveFiltersToStore: function() {
        var filtersToSave = $.map(this.currentFilterInfo[this.viewName], function(item) {
            if (item.saveToStore)
                return item;
        });

        var viewFiltersToSave = {};

        viewFiltersToSave[this.viewName] = filtersToSave;
        window.localStorage.setItem(CUSTOM_FILTER_KEY, JSON.stringify($.extend({}, this.currentFilterInfo, viewFiltersToSave)));
    },
    removeFilterByName: function(filterName) {
        if (!filterName) {
            if (!this.currentFilterInfo || this.currentFilterInfo[this.viewName].length === 0)
                this.currentFilterInfo[this.viewName] = [];
            return;
        }

        var that = this,
            filterIndex = this.getIndexByName(filterName);
        if((filterIndex >= 0) && this.currentFilterInfo[this.viewName].length !== 0) {
            that.currentFilterInfo[that.viewName].splice(filterIndex, 1);
        }
    },
    loadViewFilters: function() {
        this.currentFilterInfo = JSON.parse(window.localStorage.getItem(CUSTOM_FILTER_KEY));
    },
    renderMenu: function() {
        var that = this;
        $(that.menuSelector).empty();

        if (!that.currentFilterInfo || !that.currentFilterInfo[that.viewName] || !that.currentFilterInfo[that.viewName].length 
            || (that.currentFilterInfo[that.viewName].length === 1 && that.isFilterEmpty(that.currentFilterInfo[that.viewName][0]))) {
            $("#custom-filters").hide();
            return;
        }

        $("#custom-filters").show();

        $.each(that.currentFilterInfo[that.viewName], function(_, customFilter) {
            if (!that.isFilterEmpty(customFilter)) {
                var newItem = $("<li><a href='#" + customFilter.name + "'>" + customFilter.name + "</a><div class='update-filter'></div></li>");
                newItem.find(".update-filter").on("dxclick", function(e) {
                    EduDl.customFilters.editingFilterName = customFilter.name;
                    $("#edit-custom-filters").data("dxPopup").option("visible", true);
                    EduDl.customFilters.removeFilterByName(customFilter.name);
                });

                newItem.appendTo(that.menuSelector);
            }
        });

        if (that.currentFilterInfo[that.viewName].length >= 10) {
            $(".custom-filers-block").dxScrollView({ height: 421 });
        }
    },
    init: function(viewName) {
        var that = this;
        this.viewName = viewName;
        this.loadViewFilters();
        $(function() {
            that.renderMenu();
        });
    }
};


$(function() {
    $("#disable-db-notice").dxPopup({
        visible: false,
        width: 600,
        showTitle: false,
        showCloseButton: false,
        height: 210,
        onContentReady: function() {
            $("#disable-db-notice-ok").dxButton({
                text: "OK",
                height: 30,
                width: 70,
                onClick: function() {
                    $("#disable-db-notice").data("dxPopup").option("visible", false);
                }
            });
        }
    });


    var filterPopupRendered = $.Deferred(),
        gridElement = $("#grid"),
        pivotGridElement = $(".dashboard #revenue-analysis");
    
    function getHashValue() {
        return window.location.hash.slice(1).replace(/%20/g, " ");
    }

    $("#edit-custom-filters").dxPopup({
        visible: false,
        showTitle: false,
        showCloseButton: false,
        width: 500,
        height: 480,
        onContentReady: function() {
            $("#edit-filter-content .filter-options").dxScrollView({ direction: "both" });
            
            $("#edit-filter-content .filter-save").dxCheckBox({
                value: true
            });

            $("#edit-filter-content .filter-name").dxTextBox({
                width: 440,
                height: 30,
                placeholder: "Enter a name for your custom filter..."
            });

            $("#filters-ok").dxButton({
                text: "OK",
                height: 30,
                width: 70,
                onClick: function () {
                    var filterBuilder = $("#edit-filter-content .filter-permissions").dxFilterBuilder("instance"),
                        filterArgs = {
                            name: $("#edit-filter-content .filter-name").data("dxTextBox").option("value") || "Custom",
                            args: filterBuilder.option("value"),
                            filterExpression: filterBuilder.getFilterExpression()
                        };
                    if (!EduDl.customFilters.isFilterEmpty(filterArgs)) {
                        EduDl.customFilters.saveViewFilter(filterArgs, $("#edit-filter-content .filter-save").data("dxCheckBox").option("value"));
                        EduDl.customFilters.renderMenu();
                        window.location.hash = "#" + filterArgs.name;
                    } else {
                        if (!filterArgs || !filterArgs.name || (EduDl.customFilters.getIndexByName(filterArgs.name) == -1)) {
                            EduDl.customFilters.removeFilterByName(filterArgs.name);
                            EduDl.customFilters.renderMenu();
                            window.location.hash = "";
                        }
                    }

                    $("#edit-custom-filters").data("dxPopup").option("visible", false);
                }
            });

            $("#filters-cancel").dxButton({
                text: "Cancel",
                height: 30,
                width: 100
            });

            filterPopupRendered.resolve();
        },
        onShowing: function() {
            $.when(filterPopupRendered).done(function() {
                function buildFiltersHtml(filterData) {
                    if (!filterData || filterData.length === 0)
                        filterData = [[EduDl.DBCustomFilterFields[0].dataField, "=", ""]];

                    $("#edit-filter-content .filter-permissions").dxFilterBuilder({
                        value: filterData,
                        fields: EduDl.DBCustomFilterFields
                    });
                }
                var filterData;

                if(EduDl.customFilters.editingFilterName)
                    filterData = EduDl.customFilters.getFilterInfoByName(EduDl.customFilters.editingFilterName);

                $("#edit-filter-content .filter-name").data("dxTextBox").option("value", EduDl.customFilters.editingFilterName || "");
                $("#edit-filter-content .filter-save").data("dxCheckBox").option("value", true);

                buildFiltersHtml(filterData);

                var filterTitle = "Create Custom Filter";
                var filterOkOptions = {
                    text: "OK"
                };

                var filterCancelOptions = {
                    text: "Cancel",
                    onClick: function() {
                        $("#edit-custom-filters").data("dxPopup").option("visible", false);
                    }

                };

                if(EduDl.customFilters.editingFilterName) {
                    filterOkOptions.text = "Save";
                    filterCancelOptions = {
                        text: "Delete",
                        onClick: function() {
                            var clearHash = getHashValue() == EduDl.customFilters.editingFilterName;
                            EduDl.customFilters.saveFiltersToStore();
                            EduDl.customFilters.renderMenu();
                            EduDl.customFilters.editingFilterName = "";
                            $("#edit-custom-filters").data("dxPopup").option("visible", false);
                            if (clearHash) {
                                window.location.hash = "";
                            }
                        }
                    };
                    filterTitle = "Edit Custom Filter";
                }

                $("#filters-ok").data("dxButton").option(filterOkOptions);
                $("#filters-cancel").data("dxButton").option(filterCancelOptions);

                $("#edit-filter-content .edit-label").text(filterTitle);
            });
        }

    });

    $("#analysis-popup").dxPopup({
        visible: false,
        width: 800,
        height: "auto",
        showTitle: false,
        showCloseButton: false,
        onShown: function() {
            $("#revenue-analysis")
                .removeClass("invisible")
                .dxPivotGrid("instance")
                .updateDimensions();
        },
        onContentReady: function() {
            $("#revenue-analysis")
                .addClass("invisible")
                .dxPivotGrid({
                    allowSortingBySummary: true,
                    fieldChooser: {
                        enabled: false
                    },
                    dataSource: new DevExpress.data.PivotGridDataSource({
                        fields: [
                            {
                                caption: "State",
                                width: 120,
                                sortBySummaryField: "Percentage",
                                sortOrder: "desc",
                                selector: function(data) {
                                    return EduDl.allStatesByKey[data.Customer_Store_Locations.Customer_Store_State].Name;
                                },
                                area: "row"
                            },
                            {
                                caption: "City",
                                width: 120,
                                selector: function(data) {
                                    return data.Customer_Store_Locations.Customer_Store_City;
                                },
                                area: "row"
                            },
                            {
                                caption: "Sales",
                                dataField: "Order_Total_Amount",
                                dataType: "number",
                                summaryType: "sum",
                                format: "currency",
                                area: "data"
                            },
                            {
                                caption: "Percentage",
                                dataField: "Order_Total_Amount",
                                summaryType: "sum",
                                summaryDisplayMode: "percentOfGrandTotal",
                                area: "data"
                            }
                        ],
                        store: EduDl.stores.customer_orders,
                        expand: ["Customer_Store_Locations"],
                        select: ["Order_Total_Amount,Customer_Store_Locations/Customer_Store_City,Customer_Store_Locations/Customer_Store_State"]
                    }),
                    onCellPrepared: function(e) {
                        if (e.area === "data" && e.columnIndex === 1) {
                            var $bullet = $("<div/>").addClass("bullet").appendTo(e.cellElement.width(350));
                            $("<div/>").dxBullet({
                                showTarget: false,
                                showZeroLevel: false,
                                value: parseFloat(e.cell.text),
                                color: "#BF4E6A",
                                startScaleValue: 0,
                                endScaleValue: 100,
                                tooltip: {
                                    enabled: false
                                },
                                size: {
                                    width: 300,
                                    height: 20
                                }
                            }).appendTo($bullet);
                        }
                    },
                    showBorders: true,
                    showRowTotals: false,
                    height: 330
                });

            $("#analysis-close").dxButton({
                text: "Close",
                width: 90,
                height: 30,
                onClick: function() {
                    $("#analysis-popup").data("dxPopup").option("visible", false);
                }
            });
        }
    });


    $('.button-custom-filter').dxButton({
        width: 32,
        height: 30,
        icon: "custom-filter",
        hint: "Custom Filter",
        onClick: function() {
            $("#edit-custom-filters").data("dxPopup").option("visible", true);
        }
    });

    var throttleTimer = 0,
        searchByGrid = $(".search").dxTextBox({
            width: 220,
            height: 30,
            onValueChanged: function(searchText) {
                clearTimeout(throttleTimer);
                throttleTimer = setTimeout(function() {
                    EduDl.filterGrid(searchText.value, "search");
                }, 500);
            },
            valueChangeEvent: "keyup"
        }).dxTextBox("instance");

    $(".button-chooser").dxButton({
        width: 32,
        height: 30,
        icon: "column-chooser",
        hint: "Choose grid columns",
        onClick: function() {
            $("#grid").data("dxDataGrid").showColumnChooser();
        }
    });

    $(".button-export").dxButton({
        width: 32,
        height: 30,
        icon: "export",
        hint: "Export",
        onClick: function() {
            if(gridElement.length)
                gridElement.dxDataGrid("instance").exportToExcel();
            else
                pivotGridElement.dxPivotGrid("instance").exportToExcel();
        }
    });

    $(".button-analysis").dxButton({
        text: "Revenue Analysis",
        height: 30,
        onClick: function() {
            $("#analysis-popup").data("dxPopup").option("visible", true);
        }
    });

    EduDl.selectActiveFilter = function(value, container) {
        $(".active-left-menu-item").removeClass("active-left-menu-item");
        if (!container)
            container = $(".left-menu:not(#custom-filters) > ul a:contains(" + (value || "All") + ")");
        container.addClass("active-left-menu-item");

        if ($(".custom-filers-block.dx-scrollview").length)
            $(".custom-filers-block").dxScrollView("instance").scrollToElement($(".custom-filers-block a.active-left-menu-item"));
    };

    var mainMenuId = $(".main-menu").attr("id"),
        mainMenuItems = [{
            icon: "images/logo-" + mainMenuId.toLowerCase() + ".png",
            items: [
            {
                    text: "Сигнали за нередности",
                    value: "DeloSpecDx"
            },
            {
                text: "Архив",
                value: "ArchiveDx"
            },
                {
                    text: "Изход",
                    value: "Close"
                }
            ]
        }];

    $.each(mainMenuItems[0].items, function(i, item) {
        item.selected = item.text == mainMenuId;
    });

    $(".main-menu").dxMenu({
        items: mainMenuItems,
        showFirstSubmenuMode: "onHover",
        selectionMode: "single",
        selectByClick: true,
        cssClass: "main-menu-items",
        height: 30,
        onItemClick: function(e) {
            if (!e.itemData.items)
                if (e.itemData.value == "Close") {
                    location.href = EduDl.baseUrl;
                }
                else location.href = EduDl.baseUrl + e.itemData.value;
        }
    }).data("dxMenu");

    EduDl.filterGrid = function(value, type) {
        var isGrid = gridElement.length > 0,
            filterInstance = isGrid ? gridElement.dxDataGrid("instance") : pivotGridElement.dxPivotGrid("instance"),
            searchString = searchByGrid.option("value"),
            hashValue = window.location.hash ? getHashValue() : "",
            isHashType = (hashValue !== "") && !value;

        if(isGrid)
            filterInstance.needSelectElement = true;
        console.log('value=' + value);
        console.log('type=' + type);
        switch (type) {
           
            case "custom":
                if(isGrid)
                    filterInstance.filter(value);
                else {
                    filterInstance.getDataSource().filter(value);
                    filterInstance.getDataSource().reload();
                }
                break;
            case "search":
                filterInstance.searchByText(value);
                break;
            default:
                if(isHashType || (value && value !== "All")) {
                    if(isGrid)
                        filterInstance.filter([EduDl.filterField, "=", (isHashType ? hashValue : value)]);
                    else {
                        filterInstance.getDataSource().filter([EduDl.filterField, "=", (isHashType ? hashValue : value)]);
                        filterInstance.getDataSource().reload();
                    }
                } 
                else {
                    if(isGrid) {
                        filterInstance.clearFilter();
                        if(searchString)
                            filterInstance.searchByText(searchString);
                    } else {
                        filterInstance.getDataSource().filter(null);
                        filterInstance.getDataSource().reload();
                    }
                }
                EduDl.selectActiveFilter(isHashType ? hashValue : value);
        }
    };

    EduDl.createGrid = function(options, dataSource) {
        options.dataSource = new DevExpress.data.DataSource({
            store: dataSource.store,
            expand: dataSource.expand,
            select: dataSource.select,
            postProcess: function(data) {
                if (dataSource.store !== EduDl.stores.tasks) {
                    var grid = $("#grid").data("dxDataGrid");
                    if(data.length && grid.needSelectElement) {
                        grid.selectRows(data[0].items ? data[0].items[0][grid.option("dataSource").key()] : data[0][grid.option("dataSource").key()]);
                    }

                    grid.needSelectElement = false;
                }
                return data;
            }
        });

        var initedGrid = $("#grid").dxDataGrid(options).data("dxDataGrid");

        initedGrid.needSelectElement = true;
        EduDl.processFilter();

        return initedGrid;
    };

    EduDl.loadData = function(options) {
        //return $.ajax({
        //    url: EduDl.baseUrl + "odata/" + options.controller + (options.id ? "(" + options.id + ")/" : "/")
        //        + (options.method || "") + (options.query ? "?" + options.query : ""),
        //    dataType: "json",
        //    type: options.type || "GET",
        //    success: options.callback
        //});
        return $.ajax({
            url: EduDl.baseUrl +  options.controller + (options.id ? "(" + options.id + ")/" : "/")
                + (options.method || "") + (options.query ? "?" + options.query : ""),
            dataType: "json",
            type: options.type || "GET",
            success: options.callback
        });
    };

    EduDl.processFilter = function() {
        if (!window.location.hash) {
            EduDl.filterGrid();
            EduDl.selectActiveFilter("All");
            return;
        }

        var value = getHashValue(),
            found = false;
        $(".left-menu:not(#custom-filters) a").each(function () {
            var itemValue = $(this).text();
            if (value === '0') {
                EduDl.filterGrid("All");
                EduDl.selectActiveFilter("All");
                found = true;
                return false;
            }
            else { 
                EduDl.filterGrid(['DocTypeId', '=', value], 'custom');
                EduDl.selectActiveFilter(value);
                found = true;
                return false;
            }
           
        });

        if(found) return;

        $(EduDl.customFilters.currentFilterInfo[EduDl.customFilters.viewName]).each(function() {
            var itemValue = this.name;
            if (itemValue === value) {
                EduDl.customFilters.applyCustomFilter(this);
                EduDl.selectActiveFilter(value, $("#custom-filters a[href='#" + value + "']"));
                found = true;
                return false;
            }
        });
    };

    window.onhashchange = EduDl.processFilter;
});

EduDl.showDBNotice = function() {
    $("#disable-db-notice").data("dxPopup").option("visible", true);
};

EduDl.showValidationMessage = function(e) {
    var validationSummary = e.element.find(".dx-validationsummary"),
        validationSummaryInstance = validationSummary.dxValidationSummary("instance");

    validationSummaryInstance.option("itemTemplate", function() {
        var customValidationMessage = validationSummary.find(".custom-validation-message");
        if(!customValidationMessage.length)
           return $("<div>").addClass("custom-validation-message").text("Not all fields are correctly filled in");
    });
};

EduDl.showIe8Notice = function() {
    var Ie8Notice = "Your browser is out of date. Some features in this demo may not work properly.";

    $("body").append(
       '<div class="ie-8-notice">'
       + '<img src="images/warning.png" class="warning"/><span>'
       + Ie8Notice +
       '</span><img src="images/close.png" class="close"/></div>'
   );

    $(".close").click(function() {
        $(".ie-8-notice").hide();
    });
};

EduDl.formatPhoneNumber = function(phoneNumber) {
    return phoneNumber.replace(/(\d{3})(\d{3})(\d{4})/, "+1($1)$2-$3");
};
EduDl.formatDocType = function (docType) {
    return docType;
};


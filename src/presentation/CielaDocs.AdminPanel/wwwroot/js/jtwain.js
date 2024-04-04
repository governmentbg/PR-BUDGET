
//const for twain
var _serviceUrl = "http://localhost:8082/twain-web/";
var uploadBaseUrl = window.location.origin+"/upload";
var list = [];
var rootUrl = '';
var scannerObject = { Scaner: '', Dpi: '0', PixelType: '' };
var savedScanner = new Object();
function saveDefScanner() {
    //fill up the object with the variables
    scannerObject.Scaner = $("#ds").val();
    scannerObject.Dpi = $("#dpi").val();
    scannerObject.PixelType = $("#PixelType").val();


    try {

        localStorage.setItem("defScannerVars", JSON.stringify(scannerObject));

    } catch (e) {

        return false;
    }
}

function retrieveDefScanner() {

    try {
        savedScanner = JSON.parse(localStorage.getItem("defScannerVars"));

        if (savedScanner) {
            $("#ds").val(savedScanner.Scaner);
            $("#dpi").val(savedScanner.Dpi);
            $("#PixelType").val(savedScanner.PixelType);
        }

    } catch (e) {

        return false;
    }
}

$(document).ready(function () {
   
    $("#notify").text("Изтеглям данни...");
    $("#upload_counter").text('Сканирани стр.: ' + list.length);
    $.getJSON(_serviceUrl + "GetSources", function (sources) {
        $("#notify").text("Ready.");
        $("#ds").empty();
        let _selected;
        $.each(sources, function (i, _ds) {
            let _item;
            $("#ds").append(_item = $("<option/>").attr("value", JSON.stringify(_ds)).text("[" + _versions[_ds.Version] + ", " + _platforms[_ds.Platform] + "] " + _ds.Name));
            if (_ds.IsDefault) {
                _selected = _item;
            }
        });

        $("#ds").val(_selected.prop("value"));
        $("#ds").change(function () {
            $("#toolstrip").children().prop("disabled", true);
            $("#notify").text("Готовност за сканиране...");
            let _ds = JSON.parse($(this).val());
            $.getJSON(
                _serviceUrl + "GetCaps",
                { platform: _platforms[_ds.Platform], version: _versions[_ds.Version], sourceId: _ds.Id, caps: "IPixelType,XResolution" },
                function (_caps) {
                    $("#notify").text("Ready.");
                    [$("#PixelType"), $("#dpi")].forEach(function (_combo) { _combo.empty(); });
                    let _notSupportedCaps = ["IPixelType", "XResolution"].filter(function (_cap) {
                        return !_caps.some(function (x) {
                            return _capabilities[x.Cap] === _cap;
                        });
                    });
                    if (_notSupportedCaps.length > 0) {
                        $("#notify").text("The DS does not support all required capabilities: " + _notSupportedCaps);
                    }
                    $.each(_caps, function (i, _cap) {
                        let _combo = { IPixelType: $("#PixelType"), XResolution: $("#dpi") }[_capabilities[_cap.Cap]];
                        if (_combo !== undefined) {
                            let _current;
                            $.each(_cap.Values, function (i, _val) {
                                let _item;
                                _combo.append(_item = $("<option/>").prop("value", JSON.stringify(_val.RawValue)).text(_capValues[_capabilities[_cap.Cap]](_val.Name)));
                                if (_cap.Current.Name===_val.Name) {
                                    _current = _item;
                                }
                            });
                            _combo.val(_current.prop("value"));
                        }
                    });
                    retrieveDefScanner();
                }).fail(function () {
                    $("#notify").text("An error occurred during Get Capabilities.");
                }).always(function () {
                    $("#toolstrip").children().prop("disabled", false);
                });
        }).change();

        $("#acquire").click(function () {
            $("#toolstrip").children().prop("disabled", true);
            $("#notify").text("Сканирам...");

            let _ds = JSON.parse($("#ds").val());
            $.getJSON(
                _serviceUrl + "Acquire",
                {
                    platform: _platforms[_ds.Platform],
                    version: _versions[_ds.Version],
                    sourceId: _ds.Id,
                    caps: JSON.stringify({
                        IPixelType: $("#PixelType").val(),
                        XResolution: $("#dpi").val(),
                        YResolution: $("#dpi").val()
                    })
                },
                function (_image) {
                    $("#notify").text("Ready.");
                    $("#image").prop("src", "data:image/jpeg;base64," + _image);
                    $("#image").data({ base64: _image });
                    saveTempImage();
                }).fail(function () {
                    $("#notify").text("An error occurred during Acquire.");
                }).always(function () {
                    $("#toolstrip").children().prop("disabled", false);
                });
        });

        $("#upload").click(function () {
            let _image = $("#image").data()["base64"];
            if (_image === undefined) {
                alert("The image does not acquired yet.");
                return;
            }
            $("#toolstrip").children().prop("disabled", true);
            $("#notify").text("uploading...");
            $.post(uploadBaseUrl, { action: "create", ext: "jpg" }, function (filename) {
                let _append = function (position,blockSize) {
                    $("#notify").text("uploading... " + filename + " " + Math.round(position / _image.length * 100)+"%");
                    $.post(uploadBaseUrl, { action: "append", name: filename, data: _image.substr(position, blockSize) })
                        .done(function () {
                            if (position + blockSize < _image.length) {
                                _append(position + blockSize, blockSize);
                            } else {
                                $("#toolstrip").children().prop("disabled", false);
                                $("#notify").text("uploaded. " + filename);
                            }
                        })
                        .fail(function (xhr) {
                            console.log('upload failed.' + xhr.status + ' -' + xhr.statusText + ' -' + xhr.responseText);
                            $("#toolstrip").children().prop("disabled", false);
                            $("#notify").text("upload failed. " + filename);
                        });
                };
                _append(0, 8 * 1024);
            }).fail(function (xhr) {
                console.log('Грешка при прикачване на файл.' + xhr.status + ' -' + xhr.statusText + ' -' + xhr.responseText);
                $("#toolstrip").children().prop("disabled", false);
                $("#notify").text("upload failed.");
            });
        });
    }).fail(function () {
        $("#notify").text("An error occurred during Get Sources.");
    });
    function saveTempImage() {
        let _image = $("#image").data()["base64"];
        if (_image === undefined) {
            alert("The image does not acquired yet.");
            return;
        }
        $("#toolstrip").children().prop("disabled", true);
        $("#notify").text("записвам...");
        $.post(uploadBaseUrl, { action: "create", ext: "jpg" }, function (filename) {
            let _append = function (position, blockSize) {
                $("#notify").text("записвам... " + filename + " " + Math.round(position / _image.length * 100) + "%");
                $.post(uploadBaseUrl, { action: "append", name: filename, data: _image.substr(position, blockSize) })
                    .done(function () {
                        if (position + blockSize < _image.length) {
                            _append(position + blockSize, blockSize);
                        } else {
                            $("#toolstrip").children().prop("disabled", false);
                            $("#notify").text("Готов за сканиране. " + filename);
                            list.push(filename);
                            $("#upload_counter").text('Сканирани стр.: ' + list.length);
                          
                        }
                    })
                    .fail(function (xhr) {
                        console.log('upload failed.' + xhr.status + ' -' + xhr.statusText + ' -' + xhr.responseText);
                        $("#toolstrip").children().prop("disabled", false);
                        $("#notify").text("upload failed. " + filename);
                    });
            };
            _append(0, 8 * 1024);
        }).fail(function (xhr) {
            console.log('Грешка при прикачване на файл.' + xhr.status + ' -' + xhr.statusText + ' -' + xhr.responseText);
            $("#toolstrip").children().prop("disabled", false);
            $("#notify").text("upload failed.");
        });
    }
});

var _platforms = { 0: "X86_32", 1: "X86_64" };

var _versions = { 0: "V1", 1: "V2" };

var _capabilities = {
    0x0101: "IPixelType",
    0x1118: "XResolution"
};

var _capValues = {
    IPixelType: function (value) {
        return { 0: "BW", 1: "Gray", 2: "RGB", 3: "Palette", 4: "CMY", 5: "CMYK", 6: "YUV", 7: "YUVK", 8: "CIEXYZ", 9: "LAB", 10: "SRGB", 11: "SCRGB", 16: "INFRARED" }[value];
    },
    XResolution: function (value) {
        return value;
    }
};

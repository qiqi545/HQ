"use strict";
var d = document;
var c = console;
var ui = new signalR.HubConnectionBuilder().withUrl("/ui").build();
ui.start().then(function () { }).catch(function (e) {
    return console.error(e.toString());
});
ui.on("f", function (b, s) {
    document.getElementById("ui-body").innerHTML = b;
    document.getElementById("ui-scripts").innerHTML = `function initUi() {${s}};`;
});
ui.on("x", function (b, s) {
    const bodyId = "ui-body";
    const wb = document.createElement('div');
    wb.id = "ui-body";
    wb.innerHTML = b;
    window.setDOM(document.getElementById(bodyId), wb);

    const scriptsId = "ui-scripts";
    const ws = document.createElement('script');
    ws.id = "ui-scripts";
    ws.type = "text/javascript";
    ws.innerHTML = `function initUi() {${s}};`;
    window.setDOM(document.getElementById(scriptsId), ws);

    initUi();
});
ui.on("l", function (id, e) {
    console.log(id + " " + e);
});
ui.on("e", function (id, e) {
    console.error(id + " " + e);
});
function maybeAddListener(id, eventType, el) {
    const attr = `data-event-${eventType}`;
    if (el.getAttribute(attr) !== "1") {
        el.setAttribute(attr, "1");
        var handler;
        el.addEventListener(eventType, handler = function (e) {
            el.removeEventListener(eventType, handler);
            ui.invoke("e", window.location.toString(), id, eventType).catch(function (err) {
                return console.error(err.toString());
            });
            e.preventDefault();
        }, false);
    }
}
document.addEventListener("DOMContentLoaded", function (event) { initUi(); });
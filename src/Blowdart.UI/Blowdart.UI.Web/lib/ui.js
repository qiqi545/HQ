"use strict";
var d = document;
var c = console;
var ui = new signalR.HubConnectionBuilder().withUrl("/ui").build();
ui.start().then(function () { }).catch(function (e) {
    return c.error(e.toString());
});
ui.on("f", function (b, s) {
    d.getElementById("ui-dom").innerHTML = b;
    d.getElementById("ui-scripts").innerHTML = s;
});
ui.on("x", function (b, s) {
    //window.setDOM(document, b);
    d.getElementById("ui-dom").innerHTML = b;
    d.getElementById("ui-scripts").innerHTML = s;
    initUi();
});
ui.on("l", function (id, e) {
    c.log(id + " " + e);
});
ui.on("e", function (id, e) {
    c.error(id + " " + e);
});

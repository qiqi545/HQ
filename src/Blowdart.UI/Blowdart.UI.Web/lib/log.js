"use strict";
var d = document;
var c = console;
var log = new signalR.HubConnectionBuilder().withUrl("/server/logs").build();
log.start().then(function () { }).catch(function (e) {
    return c.error(e.toString());
});
log.on("l", function (m) {
    if(logMessage) logMessage(m);
});
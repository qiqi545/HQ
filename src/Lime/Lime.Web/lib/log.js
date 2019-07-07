"use strict";
var d = document;
var c = console;
var log = new signalR.HubConnectionBuilder().withUrl("/server/logs").build();
log.start().then(function () { }).catch(function (e) {
    return c.error(e.toString());
});
var logHandlers = logHandlers || [];
log.on("l", function (m) {
    for (let i = 0; i < logHandlers.length; i++) {
        const handler = logHandlers[i];
        if (handler)
            handler(m);
    }
});
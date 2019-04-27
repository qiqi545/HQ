"use strict";
var d = document;
var c = console;
var ui = new signalR.HubConnectionBuilder().withUrl("/ui").build();
ui.start().then(function() {
}).catch(function (e) {
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
    for (var i = 0; i < handlers.length; i++) {
        var el = document.getElementById(handlers[i].id);
        el.removeEventListener(handlers[i].eventType, handlers[i].handler, false);
    }
    handlers = [];
    window.setDOM(document.getElementById(bodyId), wb);
    Function(s)();
});
ui.on("l", function (id, e) {
    console.log(`${id} ${e}`);
});
ui.on("e", function (id, e) {
    console.error(`${id} ${e}`);
});
var handlers = [];
function maybeAddListener(id, eventType, el) {
    const attr = `data-event-${eventType}`;
    if (el.getAttribute(attr) !== "1") {
        el.setAttribute(attr, "1");
        var handler;
        el.addEventListener(eventType,
            handler = function(e) {
                el.removeEventListener(eventType, handler);
                console.log(`${attr} invoked and removed on ${id}`);
                var tracked = document.querySelectorAll('input[id]:not([id=""])');
                var ctx = [];
                for (var i = 0; i < tracked.length; i++) {
                    ctx.push({ id: tracked[i].id, type: tracked[i].type, value: tracked[i].value });
                }
                var data = JSON.stringify(ctx);
                ui.invoke("e", window.location.toString(), id, eventType, data, JSON.stringify(el.value)).catch(
                    function(err) {
                        return console.error(err.toString());
                    });
                e.preventDefault();
            },
            false);
        handlers.push({ id: id, eventType: eventType, handler: handler });
        console.log(`${attr} registered on ${id}`);
    } else {
        console.log(`${attr} already registered on ${id}`);
    }
}
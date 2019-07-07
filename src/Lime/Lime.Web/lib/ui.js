"use strict";
var d = document;
var c = console;
var ui = new signalR.HubConnectionBuilder().withUrl("/ui").build();

ui.start().then(function() {
}).catch(function(e) {
    return console.error(e.toString());
});

ui.on("f",
    function(b, s) {
        document.getElementById("ui-body").innerHTML = b;
        Function(s)();
    });

ui.on("x",
    function(b, s) {
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

ui.on("r",
    function() {
        window.location.reload(true);
    });

ui.on("l",
    function(id, e) {
        console.log(`${id} ${e}`);
    });

ui.on("e",
    function(id, e) {
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
                ui.invoke("e",
                    window.location.toString(),
                    id,
                    eventType,
                    getInputState(el.value),
                    JSON.stringify(el.value)).catch(
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

var lastInputState = {};

function getInputState(value) {
    const all = document.querySelectorAll("input");
    const inputState = [];
    for (let i = 0; i < all.length; i++) {
        const e = all[i];
        inputState.push({ id: e.id, name: e.name, type: e.type, value: e.value });
    }
    const delta = jsonpatch.compare(lastInputState, inputState);
    const buffer = [];
    if (delta.length === 1 && delta[0].op === "replace" && delta[0].value === value)
        return null;
    for (let j = 0; j < delta.length; j++) {
        const d = delta[j];
        writeOp(buffer, d.op); // add, replace, remove
        if (d.op === "remove")
            writeString(buffer, d.path);
        else {
            let capture = /^\/([0-9])+/.exec(d.path);
            if(capture !== undefined && capture.length > 1)
                buffer[buffer.length] = parseInt(capture[1]); // inputState[i]
            switch (true) {
            case /^\/[0-9]+$/.test(d.path):
                writeString(buffer, d.value.id);
                writeString(buffer, d.value.name);
                writeType(buffer, d.value.type);
                writeString(buffer, d.value.value);
                break;
            case /^\/[0-9]+\/id$/.test(d.path):
                buffer[buffer.length] = 0; // id
                writeString(buffer, d.value);
                break;
            case /^\/[0-9]+\/name$/.test(d.path):
                buffer[buffer.length] = 1; // name
                writeString(buffer, d.value);
                break;
            case /^\/[0-9]+\/type$/.test(d.path):
                buffer[buffer.length] = 2; // type
                writeType(buffer, d.value);
                break;
                case /^\/[0-9]+\/value$/.test(d.path):
                buffer[buffer.length] = 3; // value
                writeString(buffer, d.value);
                break;
            default:
                console.error(`path was ${d.path}`);
                break;
            }
        }
    }
    lastInputState = inputState;
    return buffer;
}

function writeOp(buffer, op) {
    if (op === "add")
        buffer[buffer.length] = 0;
    else if (op === "replace")
        buffer[buffer.length] = 1;
    else if (op === "remove")
        buffer[buffer.length] = 2;
}

function writeString(buffer, str) {
    if (str.length > 0) {
        const utf8 = toUTF8Array(str);
        buffer.push(utf8.length);
        Array.prototype.push.apply(buffer, utf8);
    } else {
        buffer[buffer.length] = 0;
    }
}

function writeType(buffer, type) {
    if (type.length === 0)
        buffer[buffer.length] = 0;
    switch (type) {
    case "button":
        buffer[buffer.length] = 1;
        break;
    case "checkbox":
        buffer[buffer.length] = 2;
        break;
    case "color":
        buffer[buffer.length] = 3;
        break;
    case "date":
        buffer[buffer.length] = 4;
        break;
    case "datetimelocal":
        buffer[buffer.length] = 5;
        break;
    case "email":
        buffer[buffer.length] = 6;
        break;
    case "file":
        buffer[buffer.length] = 7;
        break;
    case "hidden":
        buffer[buffer.length] = 8;
        break;
    case "image":
        buffer[buffer.length] = 9;
        break;
    case "month":
        buffer[buffer.length] = 10;
        break;
    case "number":
        buffer[buffer.length] = 11;
        break;
    case "password":
        buffer[buffer.length] = 12;
        break;
    case "radio":
        buffer[buffer.length] = 13;
        break;
    case "range":
        buffer[buffer.length] = 14;
        break;
    case "reset":
        buffer[buffer.length] = 15;
        break;
    case "search":
        buffer[buffer.length] = 16;
        break;
    case "submit":
        buffer[buffer.length] = 17;
        break;
    case "tel":
        buffer[buffer.length] = 18;
        break;
    case "text":
        buffer[buffer.length] = 19;
        break;
    case "time":
        buffer[buffer.length] = 20;
        break;
    case "url":
        buffer[buffer.length] = 21;
        break;
    case "week":
        buffer[buffer.length] = 22;
        break;
    default:
        console.error(`type was ${type}`);
        break;
    }
}

// https://gist.github.com/jchook/f665a0d5096ab0283c3f51bab57ff132
function toUTF8Array(str) {
    const utf8 = [];
    for (let i = 0; i < str.length; i++) {
        let charcode = str.charCodeAt(i);
        if (charcode < 0x80) utf8.push(charcode);
        else if (charcode < 0x800) {
            utf8.push(0xc0 | charcode >> 6, 0x80 | charcode & 0x3f);
        } else if (charcode < 0xd800 || charcode >= 0xe000) {
            utf8.push(0xe0 | charcode >> 12, 0x80 | charcode >> 6 & 0x3f, 0x80 | charcode & 0x3f);
        }
        // surrogate pair
        else {
            i++;
            // UTF-16 encodes 0x10000-0x10FFFF by subtracting 0x10000 and splitting the 20 bits of 0x0-0xFFFFF into two halves
            charcode = 0x10000 + ((charcode & 0x3ff) << 10 | str.charCodeAt(i) & 0x3ff);
            utf8.push(0xf0 | charcode >> 18,
                0x80 | charcode >> 12 & 0x3f,
                0x80 | charcode >> 6 & 0x3f,
                0x80 | charcode & 0x3f);
        }
    }
    return utf8;
}
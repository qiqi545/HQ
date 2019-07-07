document.addEventListener("DOMContentLoaded", function (event) {
    const clear = $('#clear-logs');
    if (clear) {
        clear.click(function () {
            $("#no-logs").addClass("active");
            $("#logs").empty();
        });
    }
});
var logHandlers = logHandlers || [];
logHandlers.push(function (m) {
    var lvl;
    var icon;
    switch (m.level) {
    case 0:
        lvl = "Trace";
        icon = "circle thin";
        break;
    case 1:
        lvl = "Debug";
        icon = "terminal";
        break;
    case 2:
        lvl = "Information";
        icon = "info circle";
        break;
    case 3:
        lvl = "Warning";
        icon = "warning circle";
        break;
    case 4:
        lvl = "Error";
        icon = "remove circle";
        break;
    case 5:
        lvl = "Critical";
        icon = "bomb";
        break;
    default:
        lvl = "Unknown";
        icon = "question";
        break;
    }
    const dom = `<div class='item'><i class='large ${icon} middle aligned icon'></i><div class='content'><a class='header'>${lvl}</a><div class='description'>${m.message}</div></div></div>`;
    const feed = $("#logs");
    if (feed) {
        feed.append(dom);
        $("#no-logs").removeClass("active");
    }
});
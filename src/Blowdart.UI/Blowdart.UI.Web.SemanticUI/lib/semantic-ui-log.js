document.addEventListener("DOMContentLoaded", function (event) {
    var clear = $('#clear-logs');
    if (clear) {
        clear.click(function () {
            $('#no-logs').addClass('active');
            $('#logs').empty();
        });
    }
});
logMessage = function (m) {
    var lvl;
    switch (m.level) {
    case 0:
        lvl = "Trace";
        break;
    case 1:
        lvl = "Debug";
        break;
    case 2:
        lvl = "Information";
        break;
    case 3:
        lvl = "Warning";
        break;
    case 4:
        lvl = "Error";
        break;
    case 5:
        lvl = "Critical";
        break;
    default:
        lvl = "Unknown";
        break;
    }
    var div =
        "<div class='item'>" +
            "<i class='large envelope middle aligned icon'></i>" +
            "<div class='content'>" +
            "<a class='header'>" + lvl + "</a>" +
            "<div class='description'>" + m.message + "</div>" +
            "</div>" +
            "</div>";
    var feed = $("#logs");
    if (feed !== null) {
        feed.append(div);
        $("#no-logs").removeClass("active");
    }
};
(function () {
    "use strict";
    var detail = document.querySelector("[data-activity-detail]");
    var heading = detail && detail.querySelector("[data-detail-heading]");
    if (heading) heading.focus();
}());

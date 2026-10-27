(function () {
    "use strict";

    var chart = document.getElementById("courseCreationChart");
    var dataField = document.querySelector("input[id$='hfTrendData']");
    if (!chart || !dataField) {
        return;
    }

    var points;
    try {
        points = JSON.parse(dataField.value || "[]");
    } catch (error) {
        points = [];
    }

    if (!points.length) {
        chart.textContent = "No course creation data for this period.";
        chart.classList.add("cr-inline-empty");
        return;
    }

    var maximum = points.reduce(function (largest, point) {
        return Math.max(largest, Number(point.count) || 0);
    }, 0);
    var summary = [];

    chart.style.gridTemplateColumns = "repeat(" + points.length + ", minmax(32px, 1fr))";
    chart.style.minWidth = Math.max(520, points.length * 42) + "px";

    points.forEach(function (point) {
        var count = Number(point.count) || 0;
        var item = document.createElement("div");
        var bar = document.createElement("div");
        var value = document.createElement("span");
        var label = document.createElement("span");

        item.className = "cr-chart-item";
        bar.className = "cr-chart-bar";
        value.textContent = count;
        label.className = "cr-chart-label";
        label.textContent = point.label;
        bar.style.height = (maximum === 0 ? 2 : Math.max(2, count / maximum * 180)) + "px";
        bar.title = point.label + ": " + count + (count === 1 ? " course created" : " courses created");
        bar.appendChild(value);
        item.appendChild(bar);
        item.appendChild(label);
        chart.appendChild(item);
        summary.push(point.label + ": " + count);
    });

    chart.setAttribute("aria-label", "Courses created over time. " + summary.join(", "));
}());

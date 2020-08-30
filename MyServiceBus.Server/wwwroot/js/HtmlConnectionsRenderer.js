var HtmlConnectionsRenderer = /** @class */ (function () {
    function HtmlConnectionsRenderer() {
    }
    HtmlConnectionsRenderer.renderConnectionsTable = function (r) {
        return ('<table class="table table-striped">' +
            "<tr>" +
            "<th>Name<div>Data Per sec</div></th>" +
            "<th>Info</th>" +
            "<th>Ip</th>" +
            "<th>Id</th>" +
            "<th>Topics</th>" +
            "<th>Queues</th>" +
            "</tr>" +
            "<tbody>" +
            this.renderTableContent(r) +
            "</tbody>" +
            "</table>");
    };
    HtmlConnectionsRenderer.renderTableContent = function (r) {
        var itm = "";
        for (var _i = 0, r_1 = r; _i < r_1.length; _i++) {
            var el = r_1[_i];
            var topics = "";
            if (el.topics)
                for (var _a = 0, _b = el.topics; _a < _b.length; _a++) {
                    var topic = _b[_a];
                    topics += '<span class="badge badge-secondary">' + topic + "</span>";
                }
            var queues = "";
            if (el.queues)
                for (var _c = 0, _d = el.queues; _c < _d.length; _c++) {
                    var queue = _d[_c];
                    queues += '<span class="badge badge-secondary">' + queue + "</span>";
                }
            itm +=
                "<tr>" +
                    "<td>" +
                    Utils.renderName(el.name) +
                    "<div>Pub:" +
                    el.publishPacketsPerSecond +
                    "</div>" +
                    "<div>Sub:" +
                    el.publishPacketsPerSecond +
                    "</div>" +
                    "<div>Total:" +
                    el.packetsPerSecondInternal +
                    "</div></td>" +
                    "<td>" +
                    "<div> Connected:" +
                    el.connectedTimeStamp +
                    "</div>" +
                    "<div> Last Recv Time:" +
                    el.receiveTimeStamp +
                    "</div>" +
                    "<div> Read bytes:" +
                    Utils.renderBytes(el.receivedBytes) +
                    "</div>" +
                    "<div> Sent bytes:" +
                    Utils.renderBytes(el.sentBytes) +
                    "</div>" +
                    "<div> Ver:" +
                    el.protocolVersion +
                    "</div>" +
                    "</td>" +
                    "<td>" +
                    el.ip +
                    "</td>" +
                    "<td>" +
                    el.id +
                    "</td>" +
                    "<td>" +
                    topics +
                    "</td>" +
                    "<td>" +
                    queues +
                    "</td>" +
                    "</tr>";
        }
        return itm;
    };
    return HtmlConnectionsRenderer;
}());
//# sourceMappingURL=HtmlConnectionsRenderer.js.map
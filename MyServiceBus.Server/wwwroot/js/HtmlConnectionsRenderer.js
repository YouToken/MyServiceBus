var HtmlConnectionsRenderer = /** @class */ (function () {
    function HtmlConnectionsRenderer() {
    }
    HtmlConnectionsRenderer.renderConnectionsTable = function (r) {
        return ('<table class="table table-striped">' +
            "<tr>" +
            "<th>Id</th>" +
            "<th>Name<div>Data Per sec</div></th>" +
            "<th>Info</th>" +
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
                    topics += '<div><span class="badge badge-secondary">' + topic + "</span></div>";
                }
            var queues = "";
            if (el.queues)
                for (var _c = 0, _d = el.queues; _c < _d.length; _c++) {
                    var queue = _d[_c];
                    var leasedQueue = ' <span class="badge badge-warning">Leased:' + HtmlCommonRenderer.RenderQueueSlices(queue.leased) + '</span>';
                    queues += '<div><span class="badge badge-secondary">' + queue.id + '</span>' + leasedQueue + '<div><hr/>';
                }
            itm +=
                "<tr>" +
                    "<td>" +
                    el.id +
                    "</td>" +
                    "<td>" +
                    Utils.renderName(el.name) +
                    "<div><b>Ip:</b>" +
                    el.ip +
                    "</div>" +
                    "<div><b>Pub:</b>" +
                    el.publishPacketsPerSecond +
                    "</div>" +
                    "<div><b>Sub:</b>" +
                    el.subscribePacketsPerSecond +
                    "</div>" +
                    "<div><b>Total:</b>" +
                    el.packetsPerSecondInternal +
                    "</div></td>" +
                    '<td style="font-size:10px">' +
                    "<div><b>Connected:</b>" +
                    el.connectedTimeStamp +
                    "</div>" +
                    "<div><b>Last Recv Time:</b>" +
                    el.receiveTimeStamp +
                    "</div>" +
                    "<div><b>Read bytes:</b>" +
                    Utils.renderBytes(el.receivedBytes) +
                    "</div>" +
                    "<div><b>Sent bytes:</b>" +
                    Utils.renderBytes(el.sentBytes) +
                    "</div>" +
                    "<div><b>Last send duration:</b>" +
                    el.lastSendDuration +
                    "</div>" +
                    "<div><b>Ver:</b>" +
                    el.protocolVersion +
                    "</div>" +
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
    HtmlConnectionsRenderer.renderConnections = function (connections) {
        var result = "";
        for (var _i = 0, connections_1 = connections; _i < connections_1.length; _i++) {
            var connection = connections_1[_i];
            result += this.renderConnection(connection);
        }
        return result;
    };
    HtmlConnectionsRenderer.renderConnection = function (conn) {
        var topics = "";
        for (var _i = 0, _a = conn.topics; _i < _a.length; _i++) {
            var topic = _a[_i];
            topics += HtmlCommonRenderer.renderBadge('secondary', topic) + " ";
        }
        return '<tr><td>' + conn.id + '</td>' +
            '<td style="font-size: 8px">' +
            '<div>' + conn.name + '</div>Ip:' + conn.ip + '</td>' +
            '<td>' + topics + '</td>' +
            '<td></td>' +
            '</tr>';
    };
    return HtmlConnectionsRenderer;
}());
//# sourceMappingURL=HtmlConnectionsRenderer.js.map
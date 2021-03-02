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
                    el.deliveryPacketsPerSecond +
                    "</div>" +
                    "</td>" +
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
            topics += HtmlCommonRenderer.renderBadge('secondary', topic) + "<br/>";
        }
        var queues = "";
        for (var _b = 0, _c = conn.queues; _b < _c.length; _b++) {
            var queue = _c[_b];
            var qSize = Utils.getQueueSize(queue.leased);
            queues += HtmlCommonRenderer.renderBadge('secondary', queue.topicId + ">>>" + queue.queueId) +
                HtmlCommonRenderer.renderBadge(Utils.queueIsEmpty(queue.leased)
                    ? 'warning'
                    : 'danger', 'Leased:' + HtmlCommonRenderer.RenderQueueSlices(queue.leased)) + 'Size: ' + qSize +
                "<hr/>";
        }
        return '<tr><td>' + conn.id + '</td>' +
            '<td style="font-size: 10px">' +
            HtmlCommonRenderer.renderClientName(conn.name) +
            '<div><b>Ip</b>:' + conn.ip + '</div>' +
            '<div><b>Connected</b>:' + conn.connected + '</div>' +
            '<div><b>Last incoming:</b>:' + conn.recv + '</div>' +
            '<div><b>Read bytes:</b>:' + Utils.renderBytes(conn.readBytes) + '</div>' +
            '<div><b>Sent bytes:</b>:' + Utils.renderBytes(conn.sentBytes) + '</div>' +
            '<div><b>Delivery evnt per sec:</b>:' + conn.deliveryEventsPerSecond + '</div>' +
            '</td>' +
            '<td>' + topics + '</td>' +
            '<td>' + queues + '</td>' +
            '</tr>';
    };
    HtmlConnectionsRenderer.renderTopicsConnections = function (connections) {
        var result = {};
        for (var _i = 0, connections_2 = connections; _i < connections_2.length; _i++) {
            var conn = connections_2[_i];
            for (var _a = 0, _b = conn.topics; _a < _b.length; _a++) {
                var topic = _b[_a];
                if (!result[topic])
                    result[topic] = "";
                result[topic] += HtmlCommonRenderer.renderClientName(conn.name) + '<div>' + conn.ip + '</div><hr/>';
            }
        }
        for (var _c = 0, _d = Object.keys(result); _c < _d.length; _c++) {
            var topic = _d[_c];
            var el = document.getElementById('topic-connections-' + topic);
            if (el)
                el.innerHTML = result[topic];
        }
    };
    return HtmlConnectionsRenderer;
}());
//# sourceMappingURL=HtmlConnectionsRenderer.js.map
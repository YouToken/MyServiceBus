var HtmlTopicRenderer = /** @class */ (function () {
    function HtmlTopicRenderer() {
    }
    HtmlTopicRenderer.renderTable = function (r, c) {
        return ('<table class="table table-striped">' +
            "<tr>" +
            " <th>Topic</th>" +
            "<th>Size</th>" +
            "<th>Topic connections</th>" +
            "<th>Queue</th>" +
            "</tr>" +
            "<tbody>" +
            this.renderTableData(r, c) +
            "</tbody>" +
            "</table>");
    };
    HtmlTopicRenderer.toDuration = function (v) {
        return (v / 1000).toFixed(3) + "ms";
    };
    HtmlTopicRenderer.renderTableData = function (r, c) {
        var itm = "";
        for (var _i = 0, r_1 = r; _i < r_1.length; _i++) {
            var el = r_1[_i];
            itm +=
                "<tr><td><b>" +
                    el.id +
                    "</b><div>Msg/sec: " +
                    el.msgPerSec +
                    "</div><div>Req/sec: " +
                    el.requestsPerSec +
                    "</div>" +
                    "<hr/><div>Cached:</div><div>" +
                    this.renderCachedPages(el) +
                    "</div><div>" +
                    HtmlCommonRenderer.renderGraph(el.messagesPerSecond, function (v) { return v.toFixed(0); }) +
                    "</div>" +
                    "<td>" +
                    el.size +
                    "</td>" +
                    "<td>" +
                    this.renderTopicConnections(el, c) +
                    "</td>" +
                    "<td>" +
                    this.renderQueues(el.consumers) +
                    "</td>" +
                    "</tr>";
        }
        return itm;
    };
    HtmlTopicRenderer.renderTopicConnections = function (t, c) {
        var itm = "";
        for (var _i = 0, _a = t.publishers; _i < _a.length; _i++) {
            var pubId = _a[_i];
            var con = Utils.findConnection(c, pubId);
            if (con)
                itm += "<div>" + Utils.renderName(con.name) + con.ip + "</div>";
        }
        return itm;
    };
    HtmlTopicRenderer.renderCachedPages = function (r) {
        var result = "";
        for (var _i = 0, _a = r.cachedPages; _i < _a.length; _i++) {
            var info = _a[_i];
            result +=
                '<span class="badge badge-secondary" style="margin-right: 5px">' +
                    info +
                    "</span>";
        }
        return result;
    };
    HtmlTopicRenderer.renderQueues = function (c) {
        var _this = this;
        var itm = "";
        for (var _i = 0, c_1 = c; _i < c_1.length; _i++) {
            var el = c_1[_i];
            var deleteOnDisconnectBadge = el.deleteOnDisconnect
                ? '<span class="badge badge-success">auto-delete</span>'
                : '<span class="badge badge-warning">permanent</span>';
            var connectsBadge = el.connections == 0
                ? '<span class="badge badge-danger">Connects:' +
                    el.connections +
                    "</span>"
                : '<span class="badge badge-primary">Connects:' +
                    el.connections +
                    "</span>";
            var sizeBadge = el.queueSize < 1000
                ? '<span class="badge badge-success">size:' + el.queueSize + "</span>"
                : '<span class="badge badge-danger">size:' + el.queueSize + "</span>";
            var readSlicesBadge = '<span class="badge badge-success">QReady:' + HtmlCommonRenderer.RenderQueueSlices(el.readySlices) + '</span>';
            var leasedAmount = '<span class="badge badge-warning">Leased:' + el.leasedAmount + '</span>';
            itm += '<table style="width:100%"><tr><td style="width: 100%">' +
                "<div>" + el.queueId + "<span> </span>" + sizeBadge + "<span> </span>" + deleteOnDisconnectBadge + "</div>" +
                connectsBadge +
                "<span> </span>" +
                readSlicesBadge +
                "<span> </span>" +
                leasedAmount +
                "</td><td>" + HtmlCommonRenderer.renderGraph(el.executionDuration, function (v) { return _this.toDuration(v); }) + "</td></tr></table>";
        }
        return itm;
    };
    return HtmlTopicRenderer;
}());
//# sourceMappingURL=HtmlTopicRenderer.js.map
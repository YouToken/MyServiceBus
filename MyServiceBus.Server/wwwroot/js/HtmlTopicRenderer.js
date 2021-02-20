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
                    TopicQueueRenderer.renderQueues(el.consumers) +
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
    return HtmlTopicRenderer;
}());
//# sourceMappingURL=HtmlTopicRenderer.js.map
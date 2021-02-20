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
                    // el.msgPerSec +
                    "</div><div>Req/sec: " +
                    // el.requestsPerSec +
                    "</div>" +
                    "<hr/><div>Cached:</div><div>" +
                    //  this.renderCachedPages(el) +
                    "</div><div>" +
                    //   HtmlCommonRenderer.renderGraph(el.messagesPerSecond, v => v.toFixed(0)) +
                    "</div>" +
                    "<td>" +
                    el.size +
                    "</td>" +
                    "<td>" +
                    this.renderTopicConnections(el, c) +
                    "</td>" +
                    "<td>" +
                    HtmlTopicQueueRenderer.renderQueues(el.consumers) +
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
    HtmlTopicRenderer.renderCachedPages = function (pages) {
        var result = "";
        for (var _i = 0, pages_1 = pages; _i < pages_1.length; _i++) {
            var id = pages_1[_i];
            result +=
                '<span class="badge badge-secondary" style="margin-right: 5px">' +
                    id +
                    "</span>";
        }
        return result;
    };
    HtmlTopicRenderer.renderTopicsTableBody = function (topics) {
        var result = "";
        for (var _i = 0, topics_1 = topics; _i < topics_1.length; _i++) {
            var topic = topics_1[_i];
            result += '<tr>' +
                '<td><b>' + topic.id + '</b>' +
                '<div id="statistic-' + topic.id + '"></div>' +
                '<hr/>' +
                '<div style="font-size: 12px">Cached pages:</div>' +
                '<div id="cached-pages-' + topic.id + '">' + this.renderCachedPages(topic.pages) + '</div>' +
                '<div id="topic-metrics-' + topic.id + '"></div>' +
                '</td>' +
                '<td id="topic-connections-' + topic.id + '"></td>' +
                '<td id="topic-queues-' + topic.id + '"></td></tr>';
        }
        return result;
    };
    return HtmlTopicRenderer;
}());
//# sourceMappingURL=HtmlTopicRenderer.js.map
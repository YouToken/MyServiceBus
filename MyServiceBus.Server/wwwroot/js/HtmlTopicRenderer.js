var HtmlTopicRenderer = /** @class */ (function () {
    function HtmlTopicRenderer() {
    }
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
            result += '<tr class="search-filter" data-filter="' + topic.id + '">' +
                '<td><b>' + topic.id + '</b>' +
                '<div id="statistic-' + topic.id + '"></div>' +
                '<hr/>' +
                '<div style="font-size: 12px">Cached pages:</div>' +
                '<div id="cached-pages-' + topic.id + '">' + this.renderCachedPages(topic.pages) + '</div>' +
                '<div id="topic-performance-graph-' + topic.id + '"></div>' +
                '</td>' +
                '<td id="topic-connections-' + topic.id + '"></td>' +
                '<td id="topic-queues-' + topic.id + '"></td></tr>';
        }
        return result;
    };
    HtmlTopicRenderer.renderRequestsPerSecond = function (data) {
        return '<div>Msg/sec:' + data.msgPerSec + '</div>' +
            '<div>Req/sec:' + data.reqPerSec + '</div>';
    };
    return HtmlTopicRenderer;
}());
//# sourceMappingURL=HtmlTopicRenderer.js.map
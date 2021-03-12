var HtmlCommonRenderer = /** @class */ (function () {
    function HtmlCommonRenderer() {
    }
    HtmlCommonRenderer.renderGraph = function (c, showValue, getValue, highlight) {
        var max = Utils.getMax(c);
        var w = 50;
        var coef = max == 0 ? 0 : w / max;
        var result = '<svg width="240" height="' +
            w +
            '"> <rect width="240" height="' +
            w +
            '" style="fill:none;stroke-width:;stroke:black" />';
        var i = 0;
        for (var _i = 0, c_1 = c; _i < c_1.length; _i++) {
            var m = c_1[_i];
            var y = w - getValue(m) * coef;
            var highLight = highlight(m);
            if (highLight) {
                result +=
                    '<line x1="' +
                        i +
                        '" y1="' +
                        w +
                        '" x2="' +
                        i +
                        '" y2="0" style="stroke:#ed969e;stroke-width:2" />';
            }
            var color = highLight ? "red" : "lightblue";
            result +=
                '<line x1="' +
                    i +
                    '" y1="' +
                    w +
                    '" x2="' +
                    i +
                    '" y2="' +
                    y +
                    '" style="stroke:' + color + ';stroke-width:2" />';
            i += 2;
        }
        return result + '<text x="0" y="15" fill="red">' + showValue(max) + "</text></svg>";
    };
    HtmlCommonRenderer.RenderQueueSlices = function (queueSlices) {
        var result = "";
        for (var _i = 0, queueSlices_1 = queueSlices; _i < queueSlices_1.length; _i++) {
            var c = queueSlices_1[_i];
            result += c.from + "-" + c.to + "; ";
        }
        return result;
    };
    HtmlCommonRenderer.getTopicsTable = function () {
        return '<table class="table table-striped">' +
            '<tr><th>Topic</th><th>Topic connections</th><th>Queues</th><tbody id="topicTableBody"></tbody></tr>' +
            '</table>';
    };
    HtmlCommonRenderer.getTcpConnectionsDataTable = function () {
        return '<table class="table table-striped">' +
            '<tr><th>Id</th><th>Info</th><th>Topics</th><th>Queues</th><tbody id="tcpConnections"></tbody></tr>' +
            '</table>';
    };
    HtmlCommonRenderer.getSearchLayout = function () {
        return '<div><input id="input-filter" type="text" class="form-control" placeholder="Filter" style="max-width: 300px" onkeyup="Main.filter(this)"/></div>';
    };
    HtmlCommonRenderer.getMainLayout = function () {
        return this.getSearchLayout() +
            this.getTopicsTable() +
            this.getTcpConnectionsDataTable() +
            '<div id="persistent-queue"></div>';
    };
    HtmlCommonRenderer.renderBadge = function (badgeType, content) {
        return '<span class="badge badge-' + badgeType + '">' + content + '</span>';
    };
    HtmlCommonRenderer.renderBadgeWithId = function (id, badgeType, content) {
        return '<span id="' + id + '" class="badge badge-' + badgeType + '">' + content + '</span>';
    };
    HtmlCommonRenderer.toDuration = function (v) {
        return (v / 1000).toFixed(3) + "ms";
    };
    HtmlCommonRenderer.renderClientName = function (name) {
        var names = name.split(';');
        if (name.length == 1)
            return '<div>' + name + '</div>';
        return '<div><b>' + names[0] + '</b></div><div>' + names[1] + '</div>';
    };
    HtmlCommonRenderer.renderSocketLog = function (data) {
        var result = '<div class="console">';
        for (var _i = 0, data_1 = data; _i < data_1.length; _i++) {
            var itm = data_1[_i];
            result += '<div>' + itm.date + ": Connection:" + itm.name + '; Ip' + itm.ip + "; Msg:" + itm.msg;
        }
        return result + '</div>';
    };
    return HtmlCommonRenderer;
}());
//# sourceMappingURL=HtmlCommonRenderer.js.map
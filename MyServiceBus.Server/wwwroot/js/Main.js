var Main = /** @class */ (function () {
    function Main() {
    }
    Main.getMax = function (c) {
        var result = 0;
        for (var _i = 0, c_1 = c; _i < c_1.length; _i++) {
            var i = c_1[_i];
            if (i > result)
                result = i;
        }
        return result;
    };
    Main.renderGraph = function (c) {
        var max = this.getMax(c);
        var w = 50;
        var coef = max == 0 ? 0 : w / max;
        var result = '<svg width="240" height="' + w + '"> <rect width="240" height="' + w + '" style="fill:none;stroke-width:;stroke:black" />';
        var i = 0;
        for (var _i = 0, c_2 = c; _i < c_2.length; _i++) {
            var m = c_2[_i];
            var y = w - m * coef;
            result += '<line x1="' + i + '" y1="' + w + '" x2="' + i + '" y2="' + y + '" style="stroke:lightblue;stroke-width:2" />';
            i += 2;
        }
        return result + '<text x="0" y="15" fill="red">' + max + '</text></svg>';
    };
    Main.findConnection = function (c, id) {
        for (var _i = 0, c_3 = c; _i < c_3.length; _i++) {
            var con = c_3[_i];
            if (con.id === id)
                return con;
        }
    };
    Main.renderTopicConnections = function (t, c) {
        var itm = "";
        for (var _i = 0, _a = t.publishers; _i < _a.length; _i++) {
            var pubId = _a[_i];
            var con = this.findConnection(c, pubId);
            if (con)
                itm += '<div>' + con.name + "/" + con.ip + '</div>';
        }
        return itm;
    };
    Main.renderQueues = function (c) {
        var itm = "";
        for (var _i = 0, c_4 = c; _i < c_4.length; _i++) {
            var el = c_4[_i];
            var deleteOnDisconnectBadge = el.deleteOnDisconnect
                ? '<span class="badge badge-success">auto-delete</span>'
                : '<span class="badge badge-warning">permanent</span>';
            var badge = el.connections == 0
                ? '<span class="badge badge-danger">Connects:' + el.connections + '</span>'
                : '<span class="badge badge-primary">Connects:' + el.connections + '</span>';
            var sizeBadge = el.queueSize < 1000
                ? '<span class="badge badge-success">size:' + el.queueSize + '</span>'
                : '<span class="badge badge-danger">size:' + el.queueSize + '</span>';
            var readSlicesBadge = '<span class="badge badge-success">QReady:';
            for (var _a = 0, _b = el.readySlices; _a < _b.length; _a++) {
                var c_5 = _b[_a];
                readSlicesBadge += c_5.from + "-" + c_5.to + "; ";
            }
            readSlicesBadge += '</span>';
            var leasedSlicesBadge = '<span class="badge badge-warning">QLeased:';
            for (var _c = 0, _d = el.leasedSlices; _c < _d.length; _c++) {
                var c_6 = _d[_c];
                leasedSlicesBadge += c_6.from + "-" + c_6.to + "; ";
            }
            leasedSlicesBadge += '</span>';
            itm += '<div>' + el.queueId + '<p><span> </span>' + badge + '<span> </span>' + sizeBadge + '<span> </span>'
                + deleteOnDisconnectBadge + '<span> </span>' + readSlicesBadge + '<span> </span>' + leasedSlicesBadge + '</p><hr/></div>';
        }
        return itm;
    };
    Main.renderCachedPages = function (r) {
        var result = "";
        for (var _i = 0, _a = r.cachedPages; _i < _a.length; _i++) {
            var info = _a[_i];
            result += '<span class="badge badge-secondary" style="margin-right: 5px">' + info + '</span>';
        }
        return result;
    };
    Main.renderQueuesInfo = function (r, c) {
        var itm = "";
        for (var _i = 0, r_1 = r; _i < r_1.length; _i++) {
            var el = r_1[_i];
            itm += '<tr><td><b>' + el.id + '</b><div>Msg/sec: ' + el.msgPerSec + '</div><div>Req/sec: ' + el.requestsPerSec + '</div>' +
                '<hr/><div>Cached:</div><div>' + this.renderCachedPages(el) + '</div><div>' + this.renderGraph(el.messagesPerSecond) + '</div></td>' +
                '<td>' + el.size + '</td>' +
                '<td>' + this.renderTopicConnections(el, c) + '</td>' +
                '<td>' + this.renderQueues(el.consumers) + '</td>' +
                '</tr>';
        }
        return itm;
    };
    Main.renderConnections = function (r) {
        var itm = '';
        for (var _i = 0, r_2 = r; _i < r_2.length; _i++) {
            var el = r_2[_i];
            var topics = '';
            if (el.topics)
                for (var _a = 0, _b = el.topics; _a < _b.length; _a++) {
                    var topic = _b[_a];
                    topics += '<span class="badge badge-secondary">' + topic + '</span>';
                }
            var queues = '';
            if (el.queues)
                for (var _c = 0, _d = el.queues; _c < _d.length; _c++) {
                    var queue = _d[_c];
                    queues += '<span class="badge badge-secondary">' + queue + '</span>';
                }
            itm += '<tr>' +
                '<td>' + el.name + '<div>Pub:' + el.publishPacketsPerSecond + '</div>' +
                '<div>Sub:' + el.publishPacketsPerSecond + '</div>' +
                '<div>Total:' + el.packetsPerSecondInternal + '</div></td>' +
                '<td>' + el.protocolVersion + '</td>' +
                '<td>' + el.ip + '</td>' +
                '<td>' + el.id + '</td>' +
                '<td>' + el.dateTime + '</td>' +
                '<td>' + topics + '</td>' +
                '<td>' + queues + '</td>' +
                '</tr>';
        }
        return itm;
    };
    Main.renderQueueToPersist = function (data) {
        var result = "";
        for (var _i = 0, data_1 = data; _i < data_1.length; _i++) {
            var i = data_1[_i];
            result += '<div>' + i.topicId + ' = ' + i.count + '</div>';
        }
        return result;
    };
    Main.request = function () {
        var _this = this;
        $.ajax({ url: '/Monitoring' })
            .then(function (r) {
            console.log(r);
            $('#connectionData').html(_this.renderConnections(r.connections));
            $('#mydata').html(_this.renderQueuesInfo(r.topics, r.connections));
            $('#queueToPersist').html(_this.renderQueueToPersist(r.queueToPersist));
            $('#tcpCount').html(r.tcpConnections.toString());
        });
    };
    return Main;
}());
window.setInterval(function () {
    Main.request();
}, 1000);
Main.request();
//# sourceMappingURL=Main.js.map
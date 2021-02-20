var Main = /** @class */ (function () {
    function Main() {
    }
    Main.getTopicsBody = function () {
        if (!this.topicBody)
            this.topicBody = document.getElementById('topicTableBody');
        return this.topicBody;
    };
    Main.getConnectionsBody = function () {
        if (!this.connectionsBody)
            this.connectionsBody = document.getElementById('tcpConnections');
        return this.connectionsBody;
    };
    Main.initSignalR = function () {
        var _this = this;
        this.signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl("/monitoringhub")
            .build();
        this.signalRConnection.on("init", function (data) {
            document.title = data.version + " MyServiceBus";
        });
        this.signalRConnection.on("topics", function (data) {
            _this.getTopicsBody().innerHTML = HtmlTopicRenderer.renderTopicsTableBody(data);
        });
        this.signalRConnection.on("queues", function (data) {
            for (var _i = 0, _a = Object.keys(data); _i < _a.length; _i++) {
                var topicId = _a[_i];
                var queueData = data[topicId];
                var el = document.getElementById("topic-queues-" + topicId);
                if (el)
                    el.innerHTML = HtmlTopicQueueRenderer.renderTopicQueues(topicId, queueData);
            }
        });
        this.signalRConnection.on("topic-performance-graph", function (data) {
            for (var _i = 0, _a = Object.keys(data); _i < _a.length; _i++) {
                var topicId = _a[_i];
                var metrixData = data[topicId];
                var el = document.getElementById("topic-performance-graph-" + topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, function (v) { return v.toString(); });
            }
        });
        this.signalRConnection.on("queue-duration-graph", function (data) {
            for (var _i = 0, _a = Object.keys(data); _i < _a.length; _i++) {
                var topicId = _a[_i];
                var metrixData = data[topicId];
                var el = document.getElementById("queue-duration-graph-" + topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, function (v) { return HtmlCommonRenderer.toDuration(v); });
            }
        });
        this.signalRConnection.on('connections', function (data) {
            _this.getConnectionsBody().innerHTML = HtmlConnectionsRenderer.renderConnections(data);
            HtmlConnectionsRenderer.renderTopicsConnections(data);
        });
        this.signalRConnection.on('persist-queue', function (data) {
            var el = document.getElementById('persistent-queue');
            if (el)
                el.innerHTML = HtmlQueueToPersistRenderer.RenderQueueToPersistTable(data);
        });
        this.signalRConnection.on("topic-metrics", function (data) {
            for (var _i = 0, data_1 = data; _i < data_1.length; _i++) {
                var metric = data_1[_i];
                var el = document.getElementById('statistic-' + metric.id);
                if (el)
                    el.innerHTML = HtmlTopicRenderer.renderRequestsPerSecond(metric);
                el = document.getElementById('cached-pages-' + metric.id);
                if (el)
                    el.innerHTML = HtmlTopicRenderer.renderCachedPages(metric.pages);
                for (var _a = 0, _b = metric.queues; _a < _b.length; _a++) {
                    var queue = _b[_a];
                    var topicQueueId = metric.id + '-' + queue.id;
                    el = document.getElementById('size-' + topicQueueId);
                    if (el) {
                        var oldVal = el.innerHTML;
                        var newVal = queue.size.toFixed(0);
                        el.innerHTML = newVal;
                        if (newVal != oldVal) {
                            if (queue.size > 0) {
                                el.classList.remove('badge-danger');
                                el.classList.add('badge-primary');
                            }
                            else {
                                el.classList.remove('badge-primary');
                                el.classList.add('badge-danger');
                            }
                        }
                    }
                    el = document.getElementById('queue2-' + topicQueueId);
                    if (el)
                        el.innerHTML = HtmlTopicQueueRenderer.renderTopicSecondLine(queue);
                }
            }
        });
    };
    Main.timerTick = function () {
        var _this = this;
        if (!this.bodyElement) {
            this.bodyElement = document.getElementsByTagName("BODY")[0];
            this.bodyElement.innerHTML = HtmlCommonRenderer.getMainLayout();
        }
        if (!this.signalRConnection) {
            this.initSignalR();
        }
        if (this.signalRConnection.connection.connectionState != 1) {
            this.signalRConnection.start().then(function () {
                _this.connected = true;
            })
                .catch(function (err) { return console.error(err.toString()); });
        }
    };
    Main.connected = true;
    return Main;
}());
window.setInterval(function () {
    Main.timerTick();
}, 5000);
$('document').ready(function () {
    Main.timerTick();
});
//# sourceMappingURL=Main.js.map
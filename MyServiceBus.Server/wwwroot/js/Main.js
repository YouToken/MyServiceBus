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
                console.log(queueData);
                var el = document.getElementById("topic-queues-" + topicId);
                if (el)
                    el.innerHTML = HtmlTopicQueueRenderer.renderTopicQueues(topicId, queueData);
            }
        });
        this.signalRConnection.on("topic-metrics", function (data) {
            for (var _i = 0, _a = Object.keys(data); _i < _a.length; _i++) {
                var topicId = _a[_i];
                var metrixData = data[topicId];
                var el = document.getElementById("topic-metrics-" + topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, function (v) { return v.toString(); });
            }
        });
        this.signalRConnection.on("queue-metrics", function (data) {
            for (var _i = 0, _a = Object.keys(data); _i < _a.length; _i++) {
                var topicId = _a[_i];
                var metrixData = data[topicId];
                var el = document.getElementById("metrix-" + topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, function (v) { return HtmlCommonRenderer.toDuration(v); });
            }
        });
        this.signalRConnection.on('connections', function (data) {
            _this.getConnectionsBody().innerHTML = HtmlConnectionsRenderer.renderConnections(data);
            HtmlConnectionsRenderer.renderTopicsConnections(data);
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
        console.log("Connection state: " + this.signalRConnection.connection.connectionState);
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
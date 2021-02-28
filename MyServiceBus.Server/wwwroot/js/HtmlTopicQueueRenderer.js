var HtmlTopicQueueRenderer = /** @class */ (function () {
    function HtmlTopicQueueRenderer() {
    }
    HtmlTopicQueueRenderer.renderTopicQueues = function (topicId, queues) {
        var result = "";
        for (var _i = 0, queues_1 = queues; _i < queues_1.length; _i++) {
            var queue = queues_1[_i];
            result += this.renderTopicQueue(topicId, queue);
        }
        return result;
    };
    HtmlTopicQueueRenderer.renderQueueLine = function (queue) {
        var connectionBadge = HtmlCommonRenderer.renderBadge(queue.connections > 0 ? 'primary' : 'danger', '<img style="width: 10px" src="/images/plug.svg"> ' + queue.connections);
        var queueSize = Utils.getQueueSize(queue.ready);
        var queueBadge = HtmlCommonRenderer.renderBadge(queueSize > 1000 ? "danger" : "success", HtmlCommonRenderer.RenderQueueSlices(queue.ready));
        var queueTypeBadge = HtmlCommonRenderer.renderBadge('danger', 'unknown');
        switch (queue.queueType) {
            case 0:
                queueTypeBadge = HtmlCommonRenderer.renderBadge('warning', 'permanent');
                break;
            case 1:
                queueTypeBadge = HtmlCommonRenderer.renderBadge('success', 'auto-delete');
                break;
            case 2:
                queueTypeBadge = HtmlCommonRenderer.renderBadge('warning', 'permanent-single-connect');
                break;
        }
        var sizeBadge = HtmlCommonRenderer.renderBadge(queue.size > 100 ? 'danger' : 'success', "Size:" + queue.size);
        var leasedBadge = HtmlCommonRenderer.renderBadge("warning", "Leased: " + queue.leased);
        return connectionBadge + ' ' + queueTypeBadge + ' ' + sizeBadge + ' ' + queueBadge + ' ' + leasedBadge;
    };
    HtmlTopicQueueRenderer.renderTopicQueue = function (topicId, queue) {
        var topicQueueId = topicId + '-' + queue.id;
        return '<table style="width: 100%"><tr>' +
            '<td style="width: 100%">' + queue.id +
            '<div id="queue-info-' + topicQueueId + '">' + this.renderQueueLine(queue) + '</div></td>' +
            '<td style="width: 100%"><div style="font-size: 8px">Avg Event execution duration</div><div id="queue-duration-graph-' + topicQueueId + '"></div></td>' +
            '</tr></table>';
    };
    return HtmlTopicQueueRenderer;
}());
//# sourceMappingURL=HtmlTopicQueueRenderer.js.map
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
    HtmlTopicQueueRenderer.renderTopicQueue = function (topicId, queue) {
        var topicQueueId = topicId + '-' + queue.id;
        var connectionsBadge = queue.connections > 0
            ? HtmlCommonRenderer.renderBadge('primary', '<img style="width: 10px" src="/images/plug.svg"> ' + queue.connections)
            : HtmlCommonRenderer.renderBadge('danger', '<img style="width: 10px" src="/images/plug.svg"> ' + queue.connections);
        var queueTypeBadge = queue.deleteOnDisconnect
            ? HtmlCommonRenderer.renderBadge('success', 'auto-delete')
            : HtmlCommonRenderer.renderBadge('warning', 'permanent');
        var sizeBadge = HtmlCommonRenderer.renderBadgeWithId('size-' + topicQueueId, 'success', "Size:-");
        return '<table style="width: 100%"><tr>' +
            '<td style="width: 100%">' + queue.id + ' ' + connectionsBadge + ' ' + queueTypeBadge + ' ' + sizeBadge + '<div id="info-' + topicQueueId + '"></div></td>' +
            '<td style="width: 100%"><div style="font-size: 8px">Avg Event execution duration</div><div id="queue-duration-graph-' + topicQueueId + '"></div></td>' +
            '</tr></table>';
    };
    return HtmlTopicQueueRenderer;
}());
//# sourceMappingURL=HtmlTopicQueueRenderer.js.map
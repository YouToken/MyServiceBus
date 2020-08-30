var Main = /** @class */ (function () {
    function Main() {
    }
    Main.request = function () {
        var _this = this;
        if (!this.bodyElement)
            this.bodyElement = document.getElementsByTagName("BODY")[0];
        $.ajax({ url: '/Monitoring' })
            .then(function (r) {
            _this.bodyElement.innerHTML =
                HtmlTopicRenderer.renderTable(r.topics, r.connections)
                    + HtmlConnectionsRenderer.renderConnectionsTable(r.connections)
                    + HtmlQueueToPersistRenderer.RenderQueueToPersistTable(r.queueToPersist);
        });
    };
    return Main;
}());
window.setInterval(function () {
    Main.request();
}, 1000);
Main.request();
//# sourceMappingURL=Main.js.map
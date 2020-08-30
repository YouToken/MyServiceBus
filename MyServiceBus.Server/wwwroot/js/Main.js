var Main = /** @class */ (function () {
    function Main() {
    }
    Main.request = function () {
        var _this = this;
        if (!this.bodyElement)
            this.bodyElement = document.getElementsByTagName("BODY")[0];
        if (this.requesting)
            return;
        this.requesting = true;
        $.ajax({ url: '/Monitoring' })
            .then(function (r) {
            _this.requesting = false;
            _this.bodyElement.innerHTML =
                HtmlTopicRenderer.renderTable(r.topics, r.connections)
                    + HtmlConnectionsRenderer.renderConnectionsTable(r.connections)
                    + HtmlQueueToPersistRenderer.RenderQueueToPersistTable(r.queueToPersist);
        })
            .fail(function () {
            _this.requesting = false;
        });
    };
    Main.requesting = false;
    return Main;
}());
window.setInterval(function () {
    Main.request();
}, 1000);
Main.request();
//# sourceMappingURL=Main.js.map
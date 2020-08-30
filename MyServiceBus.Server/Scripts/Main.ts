


class Main{
  
    
    private static bodyElement: Element;
    
    public static request():void {

        if (!this.bodyElement)
            this.bodyElement = document.getElementsByTagName("BODY")[0];

        $.ajax({url: '/Monitoring'})
            .then((r: IMonitoringInfo) => {

                this.bodyElement.innerHTML =
                    HtmlTopicRenderer.renderTable(r.topics, r.connections)
                    + HtmlConnectionsRenderer.renderConnectionsTable(r.connections)
                    + HtmlQueueToPersistRenderer.RenderQueueToPersistTable(r.queueToPersist);

            });
    }
    
}

window.setInterval(()=>{
    Main.request();    
}, 1000);

Main.request();

